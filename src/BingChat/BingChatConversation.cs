using System.Reactive.Linq;
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
internal sealed class BingChatConversation : IBingChattable
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
    public async Task<string> AskAsync(string message)
    {
        var request = _request.ConstructInitialPayload(message);

        await using var conn = new HubConnection(
            ConnectionFactory,
            HubProtocol,
            HubEndpoint,
            new ServiceCollection().BuildServiceProvider(),
            NullLoggerFactory.Instance);

        await conn.StartAsync();

        var response = await conn
            .StreamAsync<BingChatConversationResponse>("chat", request)
            .FirstAsync();

        return BuildAnswer(response) ?? "<empty answer>";
    }

    private static string? BuildAnswer(BingChatConversationResponse response)
    {
        //Check status
        if (!response.Result.Value.Equals("Success", StringComparison.OrdinalIgnoreCase))
        {
            throw new BingChatException($"{response.Result.Value}: {response.Result.Message}");
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
}