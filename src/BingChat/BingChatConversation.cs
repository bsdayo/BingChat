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

        return BuildAnswer(response) ?? "<empty answer>";
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> StreamAsync(
        string message, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var request = _request.ConstructInitialPayload(message);
        var chan = Channel.CreateUnbounded<string>();
        var (rx, tx) = (chan.Reader, chan.Writer);

        await using var conn = await Connect(ct);

        var completedLength = 0;
        var messageId = (string?)null;
        using var updateCallback = conn.On<ChatResponse>("update", update =>
        {
            if (update.Messages is null or { Length: 0 })
                return;

            var message = update.Messages[0];
            if (message is { MessageType: not null } or { Text: null or { Length: 0 } })
                return;
            if (messageId != (messageId ??= message.MessageId))
                return;

            tx.TryWrite(message.Text[completedLength..]);
            completedLength = message.Text.Length;
        });

        var responseCallback = conn.StreamAsync<ChatResponse>("chat", request, ct)
            .FirstAsync(ct)
            .AsTask()
            .ContinueWith(response =>
            {
                // TODO: Properly format source attributions and adaptive cards, and append them at the end of the message.
                // This is best done by extracting formatting logic from one-shot BuildAnswer used by AskAsync.
                if (messageId != null)
                {
                    var completedMessage = response.Result.Messages
                        .First(msg => msg.MessageId == messageId)
                        .Text;
                    if (completedMessage!.Length > completedLength)
                        tx.TryWrite(completedMessage[completedLength..]);
                }
                tx.Complete();
            }, ct);

        await foreach (var word in rx.ReadAllAsync(ct)) yield return word;
    }

    private static string? BuildAnswer(ChatResponse response)
    {
        //Check status
        if (!string.Equals(response.Result?.Value, "Success", StringComparison.OrdinalIgnoreCase))
        {
            throw new BingChatException($"{response.Result?.Value}: {response.Result?.Message}");
        }

        //Collect messages, including of types: Chat, SearchQuery, LoaderMessage, Disengaged
        var messages = new List<string>();
        foreach (var itemMessage in response.Messages)
        {
            //Not needed
            if (itemMessage.Author != "bot") continue;
            if (itemMessage.MessageType is "InternalSearchResult" or "RenderCardRequest")
                continue;

            //Not supported
            if (itemMessage.MessageType is "GenerateContentQuery")
                continue;

            //From Text
            var text = itemMessage.Text;

            //From AdaptiveCards
            var adaptiveCards = itemMessage.AdaptiveCards;
            if (text is null && adaptiveCards?.Length > 0)
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
            if (sourceAttributions?.Length > 0)
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