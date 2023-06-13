using BingChat.Model;

namespace BingChat
{
    internal static class BingChatParser
    {
        /// <summary>
        /// Build answer string on multiple responses, merging messages with duplicate Ids.
        /// </summary>
        public static string? ParseMessage(IList<ChatResponse> responses)
        {
            bool isStreamComplete = false;
            var messageIds = new HashSet<string>();
            var messages = new List<ResponseMessage>();

            //Collect response, from newer to older
            for (int responseIndex = responses.Count - 1; responseIndex >= 0; --responseIndex)
            {
                var response = responses[responseIndex];
                if (response is null) continue;

                //Check status
                if (response.Result is not null && response.Result.Value != "Success")
                    throw new BingChatException($"{response.Result?.Value}: {response.Result?.Message}");
                if (response.Result is not null)
                    isStreamComplete = true;
                if (response.Messages is null) continue;

                //Collect messages, from newer to older
                for (int messageIndex = response.Messages.Length - 1; messageIndex >= 0; --messageIndex)
                {
                    var message = response.Messages[messageIndex];
                    if (message is null) continue;
                    if (messageIds.Contains(message.MessageId))
                        continue;
                    messageIds.Add(message.MessageId);
                    messages.Add(message);
                }
            }

            //Parse messages
            var strMessages = new List<string>();
            for (int messageIndex = messages.Count - 1; messageIndex >= 0; --messageIndex)
            {
                var message = messages[messageIndex];
                if (message is null) continue;
                //the last message in stream doesn't need source attribution
                var strMessage = ParseMessage(message, isStreamComplete || (messageIndex > 0));
                if (strMessage?.Length > 0)
                    strMessages.Add(strMessage);
            }

            return strMessages.Count > 0 ? string.Join("\n\n", strMessages) : null;
        }

        /// <summary>
        /// Build answer string on a single response.
        /// </summary>
        public static string? ParseMessage(ChatResponse response)
        {
            //Check status
            if (response.Result is not null && response.Result.Value != "Success")
                throw new BingChatException($"{response.Result?.Value}: {response.Result?.Message}");
            if (response.Messages is null) return null;

            //Parse messages
            var strMessages = new List<string>();
            foreach (var message in response.Messages)
            {
                if (message is null) continue;
                var strMessage = ParseMessage(message);
                if (strMessage?.Length > 0)
                    strMessages.Add(strMessage);
            }

            return strMessages.Count > 0 ? string.Join("\n\n", strMessages) : null;
        }

        /// <summary>
        /// Build answer string on a response message.
        /// </summary>
        private static string? ParseMessage(ResponseMessage message, bool needSourceAttribution = true)
        {
            //Not needed
            if (message.Author != "bot") return null;
            if (message.MessageType == "InternalSearchResult" ||
                message.MessageType == "RenderCardRequest")
                return null;

            //Not supported
            if (message.MessageType == "GenerateContentQuery")
                return null;

            //From Text
            var text = message.Text;

            //From AdaptiveCards
            if (text is null && message.AdaptiveCards?.Length > 0)
                text = ParseMessage(message.AdaptiveCards);

            //From MessageType
            text ??= $"<{message.MessageType}>";

            //From SourceAttributions
            if (needSourceAttribution && message.SourceAttributions?.Length > 0)
                text += "\n\n" + ParseMessage(message.SourceAttributions);

            return text;
        }

        /// <summary>
        /// Build answer string on adaptive cards.
        /// </summary>
        private static string? ParseMessage(AdaptiveCard[] cards)
        {
            if (cards.Length == 0) return null;

            var strMessages = new List<string>();
            foreach (var card in cards)
            {
                if (card is null or {Bodies : null}) continue;
                foreach (var body in card.Bodies)
                {
                    //Plain text block
                    if (body.Type == "TextBlock" && body.Text is not null)
                        strMessages.Add(body.Text);

                    //Not supported
                    //TODO: Rich text block or other type
                }
            }

            return strMessages.Count > 0 ? string.Join('\n', strMessages) : null;
        }

        /// <summary>
        /// Build answer string on source attributions.
        /// </summary>
        private static string? ParseMessage(SourceAttribution[] sources)
        {
            if (sources.Length == 0) return null;

            var strMessages = new List<string>();
            for (var sourceIndex = 0; sourceIndex < sources.Length; ++sourceIndex)
            {
                var source = sources[sourceIndex];
                strMessages.Add($"[{sourceIndex + 1}]: {source.SeeMoreUrl} \"{source.ProviderDisplayName}\"");
            }

            return strMessages.Count > 0 ? String.Join('\n', strMessages) : null;
        }
    }
}
