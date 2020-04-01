using System.Collections.Generic;
using System.Linq;

namespace Arek.RocketChat
{
    static class RocketExtensions
    {
        private static string RocketLoginMention(this string login) => "@" + RocketLogin(login);

        private static string RocketLogin(this string login) => RocketLogins.ContainsKey(login) ? RocketLogins[login] : login;

        public static string AsRocketMessageTo(this string message, IEnumerable<string> authors)
        {
            if (!authors.Any())
                return message;

            var logins = string.Join(" ", authors.Select(RocketLoginMention));
            var outgoingMessage = $"{message} - {logins}";
            return outgoingMessage;
        }

        public static string PrepareRocketLinkMessage(this string message, string linkUrl, string linkText)
        {
            return $"[{linkText}]({linkUrl}) - {message}";
        }

        public static string Indent(this string message, int level)
        {
            char nbsp = '\u00A0';
            return message.PadLeft(level * 4 + message.Length, nbsp);
        }

        public static string ToSingleMessage(this IEnumerable<string> messages)
        {
            return string.Join("\n", messages.Distinct()).Replace("\"", string.Empty);
        }
        
        private static Dictionary<string, string> RocketLogins => Configuration.Instance.Value.RocketLoginsMap;
    }
}
