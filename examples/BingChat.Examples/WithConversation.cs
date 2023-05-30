namespace BingChat.Examples;

public static partial class Examples
{
    public static async Task AskWithConversation()
    {
        // Get the Bing "_U" cookie from wherever you like
        var cookie = Environment.GetEnvironmentVariable("BING_COOKIE");

        // Construct the chat client
        var client = new BingChatClient(new BingChatClientOptions
        {
            CookieU = cookie,
            Tone = BingChatTone.Balanced,
        });

        // Create a conversation, so we can continue chatting in the same context.
        var conversation = await client.CreateConversation();

        Console.WriteLine("Please wait...");
        var firstMessage = "Do you like cats?";
        var answer = await conversation.AskAsync(firstMessage);
        Console.WriteLine($"First answer: {answer}");

        await Task.Delay(TimeSpan.FromSeconds(5));

        Console.WriteLine("Please wait...");
        var secondMessage = "What did I just say?";
        answer = await conversation.AskAsync(secondMessage);
        Console.WriteLine($"Second answer: {answer}");
    }
}