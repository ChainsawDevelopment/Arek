using System.Linq;
using System.Net;
using GitLabNotifier;
using notifier_gitlab;
using notifier_rocketchat;

namespace notifier_console_core
{
    class Program
    {
        public static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            RunOnce();
        }
        
        private static void RunOnce()
        {
            var config = Configuration.Instance.Value;
            
            var rules = config.Rules.Select(kvp => Rule.For(kvp.Key, kvp.Value)).ToArray();

            var outputMessages = new Engine()
                .Using(GitLab.ConfiguredWith(config))
                .Using(PersistentReviewerAssigner.ConfiguredWith(config))
                .Using(rules)
                .GenerateMessages()
                .Result;

            new RocketChatClient().SendMessages(outputMessages);
        }
    }
}
