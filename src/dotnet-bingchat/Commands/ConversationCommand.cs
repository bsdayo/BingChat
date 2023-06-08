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

        var tone = BingChatTone.Balanced;
        var client = (BingChatClient?)null;
        var conversation = (BingChatConversation?)null;

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
                        var cmd = text.TrimStart('/').Split(' ')[0];
                        switch (cmd)
                        {
                            case "help":
                                PrintHelp();
                                break;

                            case "reset":
                                tone = BingChatTone.Balanced;
                                client = null;
                                conversation = null;
                                break;

                            case "theme":
                                var args = text.TrimStart('/').Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (args.Length < 2)
                                {
                                    AnsiConsole.MarkupLine("[red]Please specify the theme name. (bubble|line)[/]");
                                    break;
                                }

                                if (!Enum.TryParse<ChatTheme>(args[1], ignoreCase: true, out var theme))
                                {
                                    AnsiConsole.MarkupLine(
                                        $"[red]Unknown theme {Markup.Escape(args[1])}. Valid values are bubble or line.[/]");
                                    break;
                                }

                                settings.Theme = theme;
                                break;

                            case "tone":
                                var toneArgs = text.TrimStart('/').Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (toneArgs.Length < 2)
                                {
                                    AnsiConsole.MarkupLine("[red]Please specify the tone name. (balanced|creative|precise)[/]");
                                    break;
                                }

                                if (!Enum.TryParse(text.TrimStart('/').Split(' ')[1], ignoreCase: true, out tone))
                                {
                                    AnsiConsole.MarkupLine(
                                        $"[red]Unknown tone {Markup.Escape(toneArgs[1])}. Valid values are balanced, creative or precise.[/]");
                                    break;
                                }

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
                        var pos = Console.GetCursorPosition();
                        Console.SetCursorPosition(0, pos.Top - 1); // back to the last line
                        Utils.WriteMessage(text, settings);

                        if (conversation is null)
                        {
                            client ??= Utils.GetClient(tone);

                            AnsiConsole.MarkupLine("[green]Creating new conversation...[/]");
                            conversation = await client.CreateConversation();
                        }

                        AnsiConsole.MarkupLine("[yellow]Bing is thinking...[/]");
                        var stream = conversation.StreamAsync(text);

                        // AnsiConsole.Status()
                        //     .Spinner(Spinner.Known.BouncingBar)
                        //     .Start("Bing is thinking...", _ =>
                        //         // ReSharper disable once AccessToModifiedClosure
                        //         stream = conversation.StreamAsync(text));

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
        table.AddRow("[yellow]/tone <balanced|creative|precise>[/]", "Change current tone");
        table.AddRow("[yellow]/exit[/]", "Exit chat");

        AnsiConsole.Write(table);
    }
}