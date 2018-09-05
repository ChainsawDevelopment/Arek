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
                return null;

            var logins = string.Join(" ", authors.Select(RocketLoginMention));
            var outgoinMessage = $"{message} - {logins}";
            return outgoinMessage;
        }

        public static string PrepareRocketLinkMessage(this string message, string linkUrl, string linkText)
        {
            return $"[{linkText}]({linkUrl}) - {message}";
        }
        
        private static Dictionary<string, string> RocketLogins => Configuration.Instance.Value.RocketLoginsMap;
    }
}
