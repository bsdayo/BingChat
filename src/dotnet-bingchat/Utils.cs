using System.Text;
using Spectre.Console;

namespace BingChat.Cli;

internal static class Utils
{
    public static BingChatClient GetClient(BingChatTone tone)
    {
        var cookie = Environment.GetEnvironmentVariable("BING_COOKIE");
        return new BingChatClient(new BingChatClientOptions
        {
            CookieU = string.IsNullOrWhiteSpace(cookie) ? null : cookie,
            Tone = tone,
        });
    }

    public static void WriteMessage(string message, ChatCommandSettings settings)
    {
        var text = Markup.Escape(message.Trim());
        switch (settings.Theme)
        {
            case ChatTheme.Bubble:
            default:
                var panel = new Panel(text)
                    .Header("You", Justify.Left)
                    .RoundedBorder()
                    .BorderStyle(new Style(Color.Blue));
                AnsiConsole.Write(panel);
                break;

            case ChatTheme.Line:
                var rule = new Rule("You")
                    .LeftJustified()
                    .RuleStyle(new Style(Color.Blue));
                AnsiConsole.Write(rule);
                AnsiConsole.MarkupLine(text);
                AnsiConsole.WriteLine();
                break;
        }
    }

    public static void WriteAnswer(string answer, ChatCommandSettings settings)
    {
        var text = Markup.Escape(answer.Trim());
        switch (settings.Theme)
        {
            case ChatTheme.Bubble:
            default:
                var panel = new Panel(text)
                    .Header("Bing", Justify.Left)
                    .RoundedBorder();
                AnsiConsole.Write(panel);
                break;

            case ChatTheme.Line:
                var rule = new Rule("Bing")
                    .LeftJustified();
                AnsiConsole.Write(rule);
                AnsiConsole.MarkupLine(text);
                AnsiConsole.WriteLine();
                break;
        }
    }

    public static async Task WriteAnswerStreamAsync(IAsyncEnumerable<string> answer, ChatCommandSettings settings)
    {
        switch (settings.Theme)
        {
            case ChatTheme.Bubble:
            default:
                var initialPanel = new Panel("")
                    .Header("Bing", Justify.Left)
                    .RoundedBorder();
                await AnsiConsole.Live(initialPanel).StartAsync(async ctx =>
                {
                    var sb = new StringBuilder();
                    var lastIsLineBreak = false;
                    await foreach (var segment in answer)
                    {
                        foreach (var letter in segment)
                        {
                            if (letter == '\n')
                            {
                                lastIsLineBreak = true;
                                continue;
                            }

                            if (lastIsLineBreak)
                            {
                                sb.Append("\n\n");
                                lastIsLineBreak = false;
                            }

                            sb.Append(Markup.Escape(letter.ToString()));
                            var panel = new Panel(sb.ToString())
                                .Header("Bing", Justify.Left)
                                .RoundedBorder();
                            ctx.UpdateTarget(panel);
                        }
                    }
                });
                break;

            case ChatTheme.Line:
                var writeRule = () => AnsiConsole
                    .Write(new Rule("Bing").LeftJustified());

                await foreach (var segment in answer)
                {
                    writeRule?.Invoke();
                    writeRule = null;
                    AnsiConsole.Write(Markup.Escape(segment.Replace("\n", "\n\n")));
                }

                AnsiConsole.WriteLine();
                break;
        }
    }

    public static string ReorderFootnotes(this string answer)
    {
        var parts = answer.Split("\n\n");
        var temp = parts[0];
        for (var i = 1; i < parts.Length; i++)
            parts[i - 1] = parts[i];
        parts[^1] = temp;
        return string.Join("\n\n", parts);
    }
}