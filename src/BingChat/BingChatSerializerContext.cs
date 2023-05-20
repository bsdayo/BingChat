using System.Text.Json.Serialization;

namespace BingChat;

[JsonSerializable(typeof(BingCreateConversationResponse))]
[JsonSerializable(typeof(BingChatConversationRequest))]
[JsonSerializable(typeof(BingChatConversationResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class SerializerContext : JsonSerializerContext { }