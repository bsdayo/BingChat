namespace BingChat;

public interface IBingChattable
{
    /// <summary>
    /// Ask for an answer.
    /// </summary>
    Task<string> AskAsync(string message, CancellationToken ct = default);

    /// <summary>
    /// Ask for an answer.
    /// </summary>
    /// <returns>
    /// Asynchronous stream consisting of response text words.
    /// </returns>
    IAsyncEnumerable<string> StreamAsync(string message, CancellationToken ct = default);
}
