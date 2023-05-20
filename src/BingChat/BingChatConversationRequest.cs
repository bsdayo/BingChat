using System.Text.Json.Serialization;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618

namespace BingChat;

internal sealed class BingChatConversationRequest
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("invocationId")]
    public string InvocationId { get; set; }

    [JsonPropertyName("target")]
    public string Target { get; set; }

    [JsonPropertyName("arguments")]
    public RequestArgument[] Arguments { get; set; }
}

internal sealed class RequestArgument
{
    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("optionsSets")]
    public string[] OptionsSets { get; set; }

    [JsonPropertyName("allowedMessageTypes")]
    public string[] AllowedMessageTypes { get; set; }

    [JsonPropertyName("sliceIds")]
    public string[] SliceIds { get; set; }

    [JsonPropertyName("traceId")]
    public string TraceId { get; set; }

    [JsonPropertyName("isStartOfSession")]
    public bool IsStartOfSession { get; set; }

    [JsonPropertyName("message")]
    public RequestMessage Message { get; set; }

    [JsonPropertyName("tone")]
    public string Tone { get; set; }

    [JsonPropertyName("conversationSignature")]
    public string ConversationSignature { get; set; }

    [JsonPropertyName("participant")]
    public Participant Participant { get; set; }

    [JsonPropertyName("conversationId")]
    public string ConversationId { get; set; }
}

internal sealed class RequestMessage
{
    // Are these needed?
    // locale = ,
    // market = ,
    // region = ,
    // location = ,
    // locationHints: [],

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("inputMethod")]
    public string InputMethod { get; set; }

    [JsonPropertyName("messageType")]
    public string MessageType { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }
}

internal struct Participant
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}
