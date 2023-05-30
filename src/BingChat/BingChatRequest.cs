using BingChat.Model;

namespace BingChat;

internal sealed class BingChatRequest
{
    private readonly string _conversationId;
    private readonly string _clientId;
    private readonly string _conversationSignature;
    private readonly BingChatTone _tone;
    private int _invocationId;

    internal BingChatRequest(
        string clientId, string conversationId, string conversationSignature, BingChatTone tone)
    {
        _clientId = clientId;
        _conversationId = conversationId;
        _conversationSignature = conversationSignature;
        _tone = tone;
    }

    /// <summary>
    /// Construct the initial payload for each message
    /// </summary>
    /// <param name="message">User message to Bing Chat</param>
    internal ChatRequest ConstructInitialPayload(string message)
    {
        var traceId = Utils.GenerateRandomHexString();

        var payload = new ChatRequest
        {
            Source = "cib",
            OptionsSets = _tone switch
            {
                BingChatTone.Creative => BingChatConstants.CreativeOptionSets,
                BingChatTone.Precise => BingChatConstants.PreciseOptionSets,
                BingChatTone.Balanced or _ => BingChatConstants.BalancedOptionSets
            },
            AllowedMessageTypes = BingChatConstants.AllowedMessageTypes,
            SliceIds = Array.Empty<string>(),
            TraceId = traceId,
            IsStartOfSession = _invocationId == 0,
            Message = new RequestMessage
            {
                Timestamp = DateTime.Now,
                Author = "user",
                InputMethod = "Keyboard",
                MessageType = "Chat",
                Text = message
            },
            Tone = _tone.ToString(),
            ConversationSignature = _conversationSignature,
            Participant = new() { Id = _clientId },
            ConversationId = _conversationId
        };

        _invocationId++;
        return payload;
    }
}