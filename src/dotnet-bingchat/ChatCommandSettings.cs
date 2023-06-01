using System.ComponentModel;
using Spectre.Console.Cli;

namespace BingChat.Cli;

public abstract class ChatCommandSettings : CommandSettings
{
    [CommandOption("-t|--theme")]
    [Description("Specify the theme to use")]
    public ChatTheme Theme { get; set; } = ChatTheme.Bubble;
}

public enum ChatTheme
{
    Bubble,
    Line
}