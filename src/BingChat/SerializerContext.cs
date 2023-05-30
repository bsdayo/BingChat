using System.Text.Json.Serialization;
using BingChat.Model;

namespace BingChat;

[JsonSerializable(typeof(BingCreateConversationResponse))]
[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class SerializerContext : JsonSerializerContext { }