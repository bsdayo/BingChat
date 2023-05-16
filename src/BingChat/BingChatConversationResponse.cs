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
    public Item Item { get; set; }
}

internal sealed class Item
{
    [JsonPropertyName("messages")]
    public Message[] Messages { get; set; }

    [JsonPropertyName("result")]
    public Result Result { get; set; }
}

internal sealed class Message
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
    public Body[] Body { get; set; }
}

internal sealed class Body
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("wrap")]
    public bool Wrap { get; set; }
}

internal sealed class Result
{
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}
