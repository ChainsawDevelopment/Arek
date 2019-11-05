using System.Collections.Generic;
using System.Linq;
using System.Net;
using Arek.Contracts;
using Newtonsoft.Json;

namespace Arek.Slack
{
    public class SlackClient
    {
        public void SendMessages(IEnumerable<IMessage> messages)
        {
            var slackClient = new WebClient();
            slackClient.UploadString(
                Configuration.Instance.Value.SlackIntegrationUrl,
                JsonConvert.SerializeObject(new { text = FormatMessagesForSlack(messages) }));
        }

        public string FormatMessagesForSlack(IEnumerable<IMessage> outputMessages)
        {
            string message;
            var messages = outputMessages as IMessage[] ?? outputMessages.ToArray();
            if (messages.Any())
            {
                message = string.Join("\n", messages.Select(msg =>
                    msg.Message.AsSlackMessageTo(msg.Recipients).PrepareSlackLinkMessage(msg.LinkUrl, msg.LinkText)));
            }
            else
            {
                message = "Empty! :sunglasses:";
            }

            return message;
        }
    }

    static class SlackExtensions
    {
        private static string SlackUserId(this string login)
        {
            if (SlackUserIds.ContainsKey(login))
                return SlackUserIds[login];

            if (SlackUserIds.ContainsKey(login.SlackLogin()))
                return SlackUserIds[login.SlackLogin()];

            return null;
        }

        private static string SlackLoginMention(this string login)
        {
            var slackUserId = SlackUserId(login);
            return slackUserId != null ? $"<@{slackUserId}|{SlackLogin(login)}>" : SlackLogin(login);
        }

        private static string SlackLogin(this string login) => SlackLogins.ContainsKey(login) ? SlackLogins[login] : login;

        public static string AsSlackMessageTo(this string message, IEnumerable<string> authors)
        {
            if (!authors.Any())
                return null;

            var logins = string.Join(" ", authors.Select(SlackLoginMention));
            var slackMessage = $"{message} - {logins}";
            return slackMessage;
        }

        public static string PrepareSlackLinkMessage(this string message, string linkUrl, string linkText)
        {
            return $"<{linkUrl}|{linkText}> - {message}";
        }
        
        private static Dictionary<string, string> SlackUserIds => Configuration.Instance.Value.SlackUserIds;
        private static Dictionary<string, string> SlackLogins => Configuration.Instance.Value.SlackLoginsMaps;
    }
}