using System.Text.Json.Serialization;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618

namespace BingChat;

internal sealed class BingChatConversationResponse
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("item")]
    public ResponseItem Item { get; set; }
}

internal sealed class ResponseItem
{
    [JsonPropertyName("messages")]
    public ResponseMessage[] Messages { get; set; }

    [JsonPropertyName("result")]
    public ResponseResult Result { get; set; }
}

internal sealed class ResponseMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("messageType")]
    public string? MessageType { get; set; }

    [JsonPropertyName("adaptiveCards")]
    public AdaptiveCard[] AdaptiveCards { get; set; }
}

internal sealed class AdaptiveCard
{
    [JsonPropertyName("body")]
    public ResponseBody[] Body { get; set; }
}

internal sealed class ResponseBody
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("wrap")]
    public bool Wrap { get; set; }
}

internal sealed class ResponseResult
{
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}
