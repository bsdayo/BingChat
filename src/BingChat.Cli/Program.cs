using BingChat.Cli.Commands;
using Spectre.Console.Cli;

var app = new CommandApp<ConversationCommand>();

app.Configure(config =>
{
    config.AddCommand<AskCommand>("ask");
});

return app.Run(args);