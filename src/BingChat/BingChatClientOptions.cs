namespace BingChat;

public sealed class BingChatClientOptions
{
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
}