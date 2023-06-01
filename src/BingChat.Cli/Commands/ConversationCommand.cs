using Spectre.Console;
using Spectre.Console.Cli;

namespace BingChat.Cli.Commands;

public sealed class ConversationCommand : AsyncCommand<ConversationCommand.Settings>
{
    public class Settings : ChatCommandSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[green bold]Welcome to Bing Chat![/]");
        AnsiConsole.MarkupLine("Enter message to chat with Bing, or enter [yellow]/help[/] to get command help.");

        var client = Utils.GetClient();
        IBingChattable? conversation = null;

        try
        {
            while (true)
            {
                try
                {
                    var text = AnsiConsole.Ask<string>("[blue bold]>[/]");
                    if (string.IsNullOrWhiteSpace(text)) continue;


                    if (text.StartsWith('/'))
                    {
                        var cmd = text.TrimStart('/');
                        switch (cmd)
                        {
                            case "help":
                                PrintHelp();
                                break;

                            case "reset":
                                conversation = null;
                                break;

                            case "theme":
                                var args = cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (args.Length < 2)
                                {
                                    AnsiConsole.MarkupLine("[red]Please specify the theme name. (bubble|line)[/]");
                                    break;
                                }

                                if (!Enum.TryParse<ChatTheme>(args[1], out var theme))
                                {
                                    AnsiConsole.MarkupLine(
                                        $"[red]Unknown theme {Markup.Escape(args[1])}. Valid values are bubble or line.[/]");
                                    break;
                                }

                                settings.Theme = theme;
                                break;

                            case "exit":
                                return 0;

                            default:
                                AnsiConsole.MarkupLine(
                                    $"[red]Unknown command {Markup.Escape(cmd)}. You can enter /help for command usage.[/]");
                                break;
                        }
                    }
                    else
                    {
                        IAsyncEnumerable<string> stream = null!;

                        var pos = Console.GetCursorPosition();
                        Console.SetCursorPosition(0, pos.Top - 1); // back to the last line
                        Utils.WriteMessage(text, settings);

                        if (conversation is null)
                        {
                            AnsiConsole.MarkupLine("[green]Creating new conversation...[/]");
                            conversation = await client.CreateConversation();
                        }

                        AnsiConsole.Status()
                            .Spinner(Spinner.Known.BouncingBar)
                            .Start("Bing is thinking...", _ =>
                                // ReSharper disable once AccessToModifiedClosure
                                stream = conversation.StreamAsync(text));

                        // if (answer.EndsWith("\n<Disengaged>"))
                        // {
                        //     conversation = null;
                        //     answer = answer.Replace("\n<Disengaged>", "\nIt might be time to move onto a new topic. Let's start over.");
                        // }

                        await Utils.WriteAnswerStreamAsync(stream, settings);
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteException(e);
                }
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 1;
        }
    }

    private static void PrintHelp()
    {
        AnsiConsole.Write(new Rule("Commands").RuleStyle(new Style(Color.Yellow)));
        var table = new Table();

        table.AddColumn("Command");
        table.AddColumn("Description");

        table.AddRow("[yellow]/help[/]", "Print command help");
        table.AddRow("[yellow]/reset[/]", "Reset current conversation");
        table.AddRow("[yellow]/theme <bubble|line>[/]", "Change current theme");
        table.AddRow("[yellow]/exit[/]", "Exit chat");

        AnsiConsole.Write(table);
    }
}