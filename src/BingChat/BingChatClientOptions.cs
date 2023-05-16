namespace BingChat;

public enum BingChatTone : int
{
    Balanced = 0,
    Creative = 1,
    Precise = 2,
}

public sealed class BingChatClientOptions
{
    /// <inheritdoc cref="CookieU"/>
    [Obsolete("Use CookieU property instead.")]
    public string? Cookie { get; set; }

    /// <summary>
    /// The _U cookie value
    /// </summary>
    public string? CookieU { get; set; }

    /// <summary>
    /// The KievRPSSecAuth cookie value
    /// </summary>
    public string? CookieKievRPSSecAuth { get; set; }

    /// <summary>
    /// The exported cookie file path
    /// </summary>
    public string? CookieFilePath { get; set; }

    /// <summary>
    /// Tone used for conversation (Default: Balanced)
    /// </summary>
    public BingChatTone Tone { get; set; }
}