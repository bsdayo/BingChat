using System.Text.Json.Serialization;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618

namespace BingChat;

public sealed class BingChatConversationResponse
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("item")]
    public Item Item { get; set; }
}

public sealed class Item
{
    [JsonPropertyName("messages")]
    public Message[] Messages { get; set; }
}

public sealed class Message
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

public sealed class AdaptiveCard
{
    [JsonPropertyName("body")]
    public Body[] Body { get; set; }
}

public sealed class Body
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("wrap")]
    public bool Wrap { get; set; }
}
