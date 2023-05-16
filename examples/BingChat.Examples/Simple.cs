namespace BingChat.Examples;

public static partial class Examples
{
    public static async Task AskSimply()
    {
        // Get the Bing "_U" cookie from wherever you like
        var cookie = Environment.GetEnvironmentVariable("BING_COOKIE");
        
        // Construct the chat client
        var client = new BingChatClient(new BingChatClientOptions
        {
            CookieU = cookie,
            Tone = BingChatTone.Balanced,
        });

        Console.WriteLine("Please wait...");
        
        var message = "Do you like cats?";
        var answer = await client.AskAsync(message);

        Console.WriteLine($"Answer: {answer}");
    }
}