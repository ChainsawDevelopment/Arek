using System.Linq;
using System.Net;
using Arek.Contracts;
using Arek.Engine;
using Arek.RocketChat;

namespace Arek.Console
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

            var outputMessages = new Engine.Engine()
                .Using(Jira.Jira.WithConfig(config.JiraBaseUrl, config.JiraToken))
                .Using(GitLab.GitLab.ConfiguredWith(config))
                .Using(PersistentReviewerAssigner.ConfiguredWith(config))
                .Using(new PersistentVerifierAssigner())
                .Using(rules)
                .GenerateMessages()
                .Result;

            new RocketChatClient().SendMessages(outputMessages);
        }
    }
}
