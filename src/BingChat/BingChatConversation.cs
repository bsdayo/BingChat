using System.Runtime.CompilerServices;
using System.Threading.Channels;
using BingChat.Model;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BingChat;

/// <summary>
/// A chat conversation, enables us to chat multiple times in the same context.
/// </summary>
public sealed class BingChatConversation : IBingChattable
{
    private static readonly HttpConnectionFactory ConnectionFactory = new(Options.Create(
        new HttpConnectionOptions
        {
            DefaultTransferFormat = TransferFormat.Text,
            SkipNegotiation = true,
            Transports = HttpTransportType.WebSockets,
            Headers =
            {
                ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                                "Chrome/113.0.0.0 Safari/537.36 Edg/113.0.1774.57"
            }
        }),
        NullLoggerFactory.Instance);

    private static readonly JsonHubProtocol HubProtocol = new(
        Options.Create(new JsonHubProtocolOptions()
        {
            PayloadSerializerOptions = SerializerContext.Default.Options
        }));

    private static readonly UriEndPoint HubEndpoint = new(new Uri("wss://sydney.bing.com/sydney/ChatHub"));

    private readonly BingChatRequest _request;

    internal BingChatConversation(
        string clientId, string conversationId, string conversationSignature, BingChatTone tone)
    {
        _request = new BingChatRequest(clientId, conversationId, conversationSignature, tone);
    }

    /// <inheritdoc/>
    public async Task<string> AskAsync(string message, CancellationToken ct = default)
    {
        var request = _request.ConstructInitialPayload(message);

        await using var conn = await Connect(ct);

        var response = await conn
            .StreamAsync<ChatResponse>("chat", request, ct)
            .FirstAsync(ct);

        return BingChatParser.ParseMessage(response) ?? "<empty answer>";
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> StreamAsync(
        string message, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var request = _request.ConstructInitialPayload(message);
        var chan = Channel.CreateUnbounded<string>();
        var (rx, tx) = (chan.Reader, chan.Writer);
        var responses = new List<ChatResponse>();

        await using var conn = await Connect(ct);

        var completedLength = 0;
        using var updateCallback = conn.On<ChatResponse>("update", update =>
        {
            responses.Add(update);
            var answer = BingChatParser.ParseMessage(responses);
            if (answer is null) return;
            if (answer.Length > completedLength)
                tx.TryWrite(answer[completedLength..]);
            completedLength = answer.Length;
        });

        var responseCallback = conn.StreamAsync<ChatResponse>("chat", request, ct)
            .FirstAsync(ct)
            .AsTask()
            .ContinueWith(response =>
            {
                responses.Add(response.Result);
                var answer = BingChatParser.ParseMessage(responses) ?? "<empty answer>";
                if (answer.Length > completedLength)
                    tx.TryWrite(answer[completedLength..]);
                tx.Complete();
            }, ct);

        await foreach (var word in rx.ReadAllAsync(ct)) yield return word;
    }

    private static async Task<HubConnection> Connect(CancellationToken ct)
    {
        var conn = new HubConnection(
            ConnectionFactory,
            HubProtocol,
            HubEndpoint,
            new ServiceCollection().BuildServiceProvider(),
            NullLoggerFactory.Instance);

        await conn.StartAsync(ct);

        return conn;
    }
}