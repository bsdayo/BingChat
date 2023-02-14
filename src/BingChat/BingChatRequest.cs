using System.Text.Json;
using System.Text.Json.Serialization;

namespace BingChat;

internal sealed class BingChatRequest
{
    private readonly string _conversationId;
    private readonly string _clientId;
    private readonly string _conversationSignature;
    private int _invocationId;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    internal BingChatRequest(
        string clientId, string conversationId, string conversationSignature)
    {
        _clientId = clientId;
        _conversationId = conversationId;
        _conversationSignature = conversationSignature;
    }

    /// <summary>
    /// Construct the initial payload for each message
    /// </summary>
    internal string ConstructInitialPayload(string message)
    {
        var bytes = new byte[16];
        Random.Shared.NextBytes(bytes);
        var traceId = BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();

        var payload = new
        {
            type = 4,
            invocationId = _invocationId.ToString(),
            target = "chat",
            arguments = new[]
            {
                new
                {
                    source = "cib",
                    optionsSets = new[]
                    {
                        "nlu_direct_response_filter",
                        "deepleo",
                        "enable_debug_commands",
                        "disable_emoji_spoken_text",
                        "responsible_ai_policy_235",
                        "enablemm",
                        "dlislog"
                    },
                    allowedMessageTypes = new[]
                    {
                        "Chat",
                        "InternalSearchQuery",
                        "InternalSearchResult",
                        "InternalLoaderMessage",
                        "RenderCardRequest",
                        "AdsQuery",
                        "SemanticSerp"
                    },
                    sliceIds = Array.Empty<string>(),
                    traceId,
                    isStartOfSession = _invocationId == 0,
                    message = new
                    {
                        // Are these needed?
                        // locale = ,
                        // market = ,
                        // region = ,
                        // location = ,
                        // locationHints: [],

                        timestamp = DateTime.Now,
                        author = "user",
                        inputMethod = "Keyboard",
                        messageType = "Chat",
                        text = message
                    },
                    conversationSignature = _conversationSignature,
                    participant = new { id = _clientId },
                    conversationId = _conversationId
                }
            }
        };
        _invocationId++;
        return JsonSerializer.Serialize(payload, JsonSerializerOptions);
    }
}