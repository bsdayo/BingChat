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
        var answer = string.Empty;

        void Cleanup()
        {
            wsClient.Stop(WebSocketCloseStatus.Empty, string.Empty).GetAwaiter().GetResult();
            wsClient.Dispose();
        }

        wsClient.MessageReceived
            .Where(msg => msg.MessageType == WebSocketMessageType.Text)
            .Select(msg => msg.Text)
            .Subscribe(text =>
            {
                try
                {
                    foreach (var part in text.Split(TerminalChar, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var document = JsonDocument.Parse(part);
                        if (!document.RootElement.TryGetProperty("type", out var typeElement))
                            continue;

                        var type = typeElement.GetInt32();

                        switch (type)
                        {
                            case 1:
                                // Update the answer
                                try
                                {
                                    if (
                                        document.RootElement.TryGetProperty("arguments", out var args)
                                        && args[0].TryGetProperty("messages", out var msgs)
                                        && msgs[0].TryGetProperty("adaptiveCards", out var cards)
                                        && cards[0].TryGetProperty("body", out var body)
                                        && body[0].TryGetProperty("text", out var txt))
                                        answer = txt.GetString();
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }

                                break;

                            case 2:
                                // Received terminal message, cleanup
                                Cleanup();
                                tcs.SetResult(answer ?? "<empty answer>");
                                return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Cleanup();
                    tcs.SetException(e);
                }
            });

        // Start the WebSocket client
        wsClient.Start().GetAwaiter().GetResult();

        // Send initial messages
        wsClient.Send("{\"protocol\":\"json\",\"version\":1}" + TerminalChar);
        wsClient.Send(_request.ConstructInitialPayload(message) + TerminalChar);

        return tcs.Task;
    }
}