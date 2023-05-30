using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Web;

namespace BingChat;

public sealed class BingChatClient : IBingChattable
{
    private readonly BingChatClientOptions _options;

    public BingChatClient(BingChatClientOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Create a chat conversation, so we can chat multiple times in the same context.
    /// </summary>
    public async Task<BingChatConversation> CreateConversation()
    {
        var requestId = Guid.NewGuid();

        var cookies = new CookieContainer();
        if (!string.IsNullOrEmpty(_options.CookieU))
        {
            cookies.Add(new Uri("https://www.bing.com"), new Cookie("_U", _options.CookieU));
        }

        if (!string.IsNullOrEmpty(_options.CookieKievRPSSecAuth))
        {
            cookies.Add(new Uri("https://www.bing.com"),
                new Cookie("KievRPSSecAuth", HttpUtility.UrlEncode(_options.CookieKievRPSSecAuth)));
        }

        if (!string.IsNullOrEmpty(_options.CookieFilePath))
        {
            //Read cookie file
            try
            {
                JsonNode? cookieJson = JsonNode.Parse(File.ReadAllText(_options.CookieFilePath));
                cookies = new CookieContainer();
                foreach (var cookieItemJson in (JsonArray)cookieJson!)
                {
                    if (cookieItemJson is null)
                        continue;
                    var domain = (string?)cookieItemJson["domain"];
                    var path = (string?)cookieItemJson["path"];
                    var name = (string?)cookieItemJson["name"];
                    var value = (string?)cookieItemJson["value"];
                    if (string.IsNullOrEmpty(domain) ||
                        string.IsNullOrEmpty(path) ||
                        string.IsNullOrEmpty(name) ||
                        string.IsNullOrEmpty(value))
                        continue;
                    cookies.Add(new Uri("https://www.bing.com"),
                        new Cookie(name, HttpUtility.UrlEncode(value), path, domain));
                }
            }
            catch
            {
                throw new BingChatException("The format of the cookie file is not supported. " +
                                            "PLease install \"Cookie Editor\" and export cookies in JSON format.");
            }
        }

        if (cookies.Count == 0)
            cookies.Add(new Uri("https://www.bing.com"), new Cookie("_U", Utils.GenerateRandomHexString()));

        using var handler = new HttpClientHandler { CookieContainer = cookies };
        using var client = new HttpClient(handler);
        var headers = client.DefaultRequestHeaders;

        headers.Add("accept-language", "en-US,en;q=0.9");
        headers.Add("sec-ch-ua", "\"Not_A Brand\";v=\"99\", \"Microsoft Edge\";v=\"109\", \"Chromium\";v=\"109\"");
        headers.Add("sec-ch-ua-arch", "\"x86\"");
        headers.Add("sec-ch-ua-bitness", "\"64\"");
        headers.Add("sec-ch-ua-full-version", "\"109.0.1518.78\"");
        headers.Add("sec-ch-ua-full-version-list",
            "\"Not_A Brand\";v=\"99.0.0.0\", \"Microsoft Edge\";v=\"109.0.1518.78\", \"Chromium\";v=\"109.0.5414.120\"");
        headers.Add("sec-ch-ua-mobile", "?0");
        headers.Add("sec-ch-ua-model", "");
        headers.Add("sec-ch-ua-platform", "\"macOS\"");
        headers.Add("sec-ch-ua-platform-version", "\"12.6.0\"");
        headers.Add("sec-fetch-dest", "empty");
        headers.Add("sec-fetch-mode", "cors");
        headers.Add("sec-fetch-site", "same-origin");
        headers.Add("x-edge-shopping-flag", "1");
        headers.Add("x-ms-client-request-id", requestId.ToString().ToLower());
        headers.Add("x-ms-useragent", "azsdk-js-api-client-factory/1.0.0-beta.1 core-rest-pipeline/1.10.0 OS/MacIntel");
        headers.Add("referer", "https://www.bing.com/search");
        headers.Add("referer-policy", "origin-when-cross-origin");

        var response = await client.GetFromJsonAsync(
            "https://www.bing.com/turing/conversation/create",
            SerializerContext.Default.BingCreateConversationResponse);

        if (response!.Result is { } errResult &&
            !errResult.Value.Equals("Success", StringComparison.OrdinalIgnoreCase))
        {
            var message = $"{errResult.Value}: {errResult.Message}.";
            if (errResult.Value == "UnauthorizedRequest")
                message += " If you confirm that the correct cookie is set and you still keep seeing this, "
                           + "maybe Bing doesn't serve your region. "
                           + "You can use a proxy and try again.";
            throw new BingChatException(message);
        }

        return new(
            response.ClientId,
            response.ConversationId,
            response.ConversationSignature,
            _options.Tone);
    }

    /// <summary>
    /// Create a one-shot conversation and ask in it. The conversation will be discarded after the operation.<br/>
    /// If you want to share the same context with multiple chat messages, use <see cref="CreateConversation"/> to create a shared conversation. 
    /// </summary>
    public async Task<string> AskAsync(string message, CancellationToken ct = default)
    {
        var conversation = await CreateConversation();
        return await conversation.AskAsync(message, ct);
    }

    /// <summary>
    /// Create a one-shot conversation and ask in it. The conversation will be discarded after the operation.<br/>
    /// If you want to share the same context with multiple chat messages, use <see cref="CreateConversation"/> to create a shared conversation.
    /// </summary>
    /// <returns>
    /// Asynchronous stream consisting of response text words.
    /// </returns>
    public async IAsyncEnumerable<string> StreamAsync(
        string message, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var conversation = await CreateConversation();
        await foreach (var word in conversation.StreamAsync(message, ct))
            yield return word;
    }
}

internal sealed class BingCreateConversationResponse
{
    internal class CreateConversationResult
    {
        public string Value { get; set; } = null!;
        public string Message { get; set; } = null!;
    }

    public CreateConversationResult? Result { get; set; }

    public string ConversationId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ConversationSignature { get; set; } = null!;
}