namespace BingChat;

public interface IBingChattable
{
    /// <summary>
    /// Ask for an answer.
    /// </summary>
    Task<string> AskAsync(string message);
}