// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618

namespace BingChat.Model;

internal sealed class ChatResponse
{
    public ResponseMessage[] Messages { get; set; }
    public ResponseResult? Result { get; set; }
}

internal sealed class ResponseMessage
{
    public string? Text { get; set; }
    public string Author { get; set; }
    public string MessageId { get; set; }
    public string? MessageType { get; set; }
    public AdaptiveCard[]? AdaptiveCards { get; set; }
    public SourceAttribution[]? SourceAttributions { get; set; }
}

internal sealed class AdaptiveCard
{
    public ResponseBody[] Body { get; set; }
}

internal sealed class ResponseBody
{
    public string Type { get; set; }
    public string? Text { get; set; }
}

internal sealed class SourceAttribution
{
    public string ProviderDisplayName { get; set; }
    public string SeeMoreUrl { get; set; }
}

internal sealed class ResponseResult
{
    public string Value { get; set; }
    public string? Message { get; set; }
}