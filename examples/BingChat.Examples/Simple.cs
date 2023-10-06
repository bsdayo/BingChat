namespace BingChat.Examples;

public static partial class Examples
{
    public static async Task AskSimply()
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
            Tone = BingChatTone.Balanced,
        });

        Console.WriteLine("Please wait...");
        
        var message = "Do you like cats?";
        var answer = await client.AskAsync(message);

        Console.WriteLine($"Answer: {answer}");
    }
}