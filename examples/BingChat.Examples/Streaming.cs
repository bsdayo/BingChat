namespace BingChat.Examples;

public static partial class Examples
{
    public static async Task AskAndStreamResponseText()
    {
        // Get the Bing "_U" cookie from wherever you like
        var cookie = Environment.GetEnvironmentVariable("BING_COOKIE");

        // Get the Bing cookie file path
        var cookieFile = Environment.GetEnvironmentVariable("BING_COOKIES_FILE");

        // Construct the chat client
        var client = new BingChatClient(new BingChatClientOptions
        {
            CookieU = cookie,
            CookieFilePath = cookieFile,
            Tone = BingChatTone.Creative,
        });

        Console.WriteLine("Please wait...\n");

        var message = "Write an example creepypasta about BingChat.";
        var responseStream = client.StreamAsync(message);

        await foreach (var word in responseStream)
        {
            Console.Write(word);
        }
    }
}