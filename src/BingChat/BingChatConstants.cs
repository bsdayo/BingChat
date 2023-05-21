namespace BingChat;

internal static class BingChatConstants
{
    internal static readonly string[] OptionSets = new[]
    {
        "nlu_direct_response_filter",
        "deepleo",
        "enable_debug_commands",
        "disable_emoji_spoken_text",
        "responsible_ai_policy_235",
        "enablemm",
        "cachewriteext",
        "e2ecachewrite",
        "nodlcpcwrite",
        "nointernalsugg",
        "saharasugg",
        "rai267",
        "sportsansgnd",
        "enablenewsfc",
        "dv3sugg",
        "autosave",
        "dlislog"
    };

    internal static readonly string[] CreativeOptionSets = OptionSets
        .Concat(new[] { "h3imaginative", "clgalileo", "gencontentv3" })
        .ToArray();

    internal static readonly string[] PreciseOptionSets = OptionSets
        .Concat(new[] { "h3precise", "clgalileo", "gencontentv3" })
        .ToArray();

    internal static readonly string[] BalancedOptionSets = OptionSets
        .Concat(new[] { "galileo" })
        .ToArray();

    internal static readonly string[] AllowedMessageTypes = new[]
    {
        "Chat",
        "InternalSearchQuery",
        "InternalSearchResult",
        "InternalLoaderMessage",
        "Disengaged",
        "ActionRequest",
        "Context",
        "Progress",
        "RenderCardRequest",
        "AdsQuery",
        "SemanticSerp",
        "GenerateContentQuery",
        "SearchQuery",
    };
}