<div align="center">

![Socialify Banner](https://socialify.git.ci/b1acksoil/BingChat/image?font=Inter&language=1&logo=https%3A%2F%2Fupload.wikimedia.org%2Fwikipedia%2Fcommons%2F9%2F9c%2FBing_Fluent_Logo.svg&name=1&owner=1&pattern=Circuit%20Board&theme=Auto&description=1)

# BingChat

.NET API wrapper for Microsoft's new AI-powered Bing Chat.

![.NET Version](https://img.shields.io/badge/.NET-6-blue)
[![NuGet Version](https://img.shields.io/nuget/v/BingChat?label=NuGet)](https://www.nuget.org/packages/BingChat)
[![License](https://img.shields.io/badge/license-MIT-brightgreen)](./LICENSE)

</div>

> **Warning**
> This library is **unofficial** and heavily depends on reverse-engineering. Use at your own risk.

- [Quick Start](#quick-start)
- [Roadmap](#roadmap)
- [Contributors](#contributors)
- [License](#license)

## Quick Start

> **Note**
> You need a valid cookie from someone who has access.
>
> To get it, you can go to Developer Tools (F12) > Application Tab > Storage > Cookies, find the cookie named `_U`, and
> copy its value.

Install this package via NuGet package manager or dotnet CLI:

```
dotnet add package BingChat 
```

Then,

```csharp
using BingChat;

// Construct the chat client
var client = new BingChatClient(new BingChatClientOptions
{
    // The "_U" cookie's value
    Cookie = cookie
});

var message = "Do you like cats?";
var answer = await client.AskAsync(message);

Console.WriteLine($"Answer: {answer}");
```

The code above sends a message to the chat AI, and gets the answer from it.

This method creates a one-shot conversation and discards it when completed.
If you want to continue chatting in the same context (just like the behavior in the web interface), you need to create a
shared conversation:

```csharp
// Create a conversation, so we can continue chatting in the same context.
var conversation = await client.CreateConversation();

var firstMessage = "Do you like cats?";
var answer = await conversation.AskAsync(firstMessage);
Console.WriteLine($"First answer: {answer}");

await Task.Delay(TimeSpan.FromSeconds(5));

var secondMessage = "What did I just say?";
answer = await conversation.AskAsync(secondMessage);
Console.WriteLine($"Second answer: {answer}");
```

## Roadmap

- [ ] Implement a command line tool to interact with Bing Chat.

## Contributors

Thanks to these contributors for developing or improving this library:

[![Contributors](https://contrib.rocks/image?repo=b1acksoil/BingChat)](https://github.com/b1acksoil/BingChat/graphs/contributors)

If you have any idea on this project, feel free to open a PR and share with us :D

## License

This project is licensed under [MIT License](./LICENSE). We ❤ open source!