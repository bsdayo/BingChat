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
        string clientId, string conversationId, string conversationSignature)
    {
        _request = new BingChatRequest(clientId, conversationId, conversationSignature);
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
            if (!response.Item.Result.Value.Equals("Success", StringComparison.OrdinalIgnoreCase))
            {
                throw new BingChatException($"{response.Item.Result.Value}: {response.Item.Result.Message}");
            }

            for (var index = response.Item.Messages.Length - 1; index >= 0; index--)
            {
                var itemMessage = response.Item.Messages[index];
                if (itemMessage.MessageType != null) continue;
                if (itemMessage.Author != "bot") continue;

                // maybe is possible to use itemMessage.Text directly, but some extra information will be lost
                return itemMessage.AdaptiveCards[0].Body[0].Text;
            }

            return null;
        }

        void OnMessageReceived(string text)
        {
            try
            {
                foreach (var part in text.Split(TerminalChar, StringSplitOptions.RemoveEmptyEntries))
                {
                    var json = JsonSerializer.Deserialize<BingChatConversationResponse>(part);

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
