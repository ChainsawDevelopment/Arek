using System;
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
            var outputMessages = new Engine()
                .Using(GitLab.ConfiguredWith(config))
                .Using(PersistentReviewerAssigner.ConfiguredWith(config))
                .Using(MissingReviewsRequestRule.ConfiguredWith(config))
                .Using(OldRequestRule.ConfiguredWith(config))
                .Using(new StillDownvotedRequestRule())
                .GenerateMessages()
                .Result;

            new RocketChatClient().SendMessages(outputMessages);
        }
    }
}
