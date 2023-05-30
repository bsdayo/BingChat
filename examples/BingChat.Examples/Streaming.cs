namespace BingChat.Examples;

public static partial class Examples
{
    public static async Task AskAndStreamResponseText()
    {
        // Get the Bing "_U" cookie from wherever you like
        var cookie = Environment.GetEnvironmentVariable("BING_COOKIE");

        // Construct the chat client
        var client = new BingChatClient(new BingChatClientOptions
        {
            CookieU = cookie,
            Tone = BingChatTone.Balanced,
        });

        Console.WriteLine("Please wait...\n");

        var message = "List the most important quotes from today's news.";
        var responseStream = client.StreamAsync(message);

        await foreach (var word in responseStream)
        {
            Console.Write(word);
        }
    }
}