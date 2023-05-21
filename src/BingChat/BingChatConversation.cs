using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text.Json;
using Websocket.Client;

namespace BingChat;

/// <summary>
/// A chat conversation, enables us to chat multiple times in the same context.
/// </summary>
internal sealed class BingChatConversation : IBingChattable
{
    private const char TerminalChar = '\u001e';
    private readonly BingChatRequest _request;

    internal BingChatConversation(
        string clientId, string conversationId, string conversationSignature, BingChatTone tone)
    {
        _request = new BingChatRequest(clientId, conversationId, conversationSignature, tone);
    }

    /// <inheritdoc/>
    public Task<string> AskAsync(string message)
    {
        var wsClient = new WebsocketClient(new Uri("wss://sydney.bing.com/sydney/ChatHub"));
        var tcs = new TaskCompletionSource<string>();

        void Cleanup()
        {
            wsClient.Stop(WebSocketCloseStatus.Empty, string.Empty).ContinueWith(t =>
            {
                if (t.IsFaulted) tcs.SetException(t.Exception!);
                wsClient.Dispose();
            });
        }

        string? GetAnswer(BingChatConversationResponse response)
        {
            //Check status
            if (!response.Item.Result.Value.Equals("Success", StringComparison.OrdinalIgnoreCase))
            {
                throw new BingChatException($"{response.Item.Result.Value}: {response.Item.Result.Message}");
            }

            //Collect messages, including of types: Chat, SearchQuery, LoaderMessage, Disengaged
            var messages = new List<string>();
            foreach (var itemMessage in response.Item.Messages)
            {
                //Not needed
                if (itemMessage.Author != "bot") continue;
                if (itemMessage.MessageType == "InternalSearchResult" ||
                    itemMessage.MessageType == "RenderCardRequest")
                    continue;

                //Not supported
                if (itemMessage.MessageType == "GenerateContentQuery")
                    continue;

                //From Text
                var text = itemMessage.Text;

                //From AdaptiveCards
                var adaptiveCards = itemMessage.AdaptiveCards;
                if (text is null && adaptiveCards is not null && adaptiveCards.Length > 0)
                {
                    var bodies = new List<string>();
                    foreach (var body in adaptiveCards[0].Body)
                    {
                        if (body.Type != "TextBlock" || body.Text is null) continue;
                        bodies.Add(body.Text);
                    }
                    text = bodies.Count > 0 ? string.Join("\n", bodies) : null;
                }

                //From MessageType
                text ??= $"<{itemMessage.MessageType}>";

                //From SourceAttributions
                var sourceAttributions = itemMessage.SourceAttributions;
                if (sourceAttributions is not null && sourceAttributions.Length > 0)
                {
                    text += "\n";
                    for (var nIndex = 0; nIndex < sourceAttributions.Length; nIndex++)
                    {
                        var sourceAttribution = sourceAttributions[nIndex];
                        text += $"\n[{nIndex + 1}]: {sourceAttribution.SeeMoreUrl} \"{sourceAttribution.ProviderDisplayName}\"";
                    }
                }

                messages.Add(text);
            }

            return messages.Count > 0 ? string.Join("\n\n", messages) : null;
        }

        void OnMessageReceived(string text)
        {
            try
            {
                foreach (var part in text.Split(TerminalChar, StringSplitOptions.RemoveEmptyEntries))
                {
                    var json = JsonSerializer.Deserialize(part, SerializerContext.Default.BingChatConversationResponse);

                    if (json is not { Type: 2 }) continue;

                    Cleanup();

                    tcs.SetResult(GetAnswer(json) ?? "<empty answer>");
                    return;
                }
            }
            catch (Exception e)
            {
                Cleanup();
                tcs.SetException(e);
            }
        }

        wsClient.MessageReceived
                .Where(msg => msg.MessageType == WebSocketMessageType.Text)
                .Select(msg => msg.Text)
                .Subscribe(OnMessageReceived);

        // Start the WebSocket client
        wsClient.Start().ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                Cleanup();
                tcs.SetException(t.Exception!);
                return;
            }

            // Send initial messages
            wsClient.Send("{\"protocol\":\"json\",\"version\":1}" + TerminalChar);
            wsClient.Send(_request.ConstructInitialPayload(message) + TerminalChar);
        });

        return tcs.Task;
    }
}
