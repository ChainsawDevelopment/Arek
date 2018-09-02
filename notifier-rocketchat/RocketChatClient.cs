using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GitLabNotifier;
using Newtonsoft.Json;

namespace notifier_rocketchat
{
    public class RocketChatClient
    {
        public void SendMessages(IEnumerable<IMessage> messages)
        {
            var chatClient = new WebClient();
            var formatedMessages = FormatMessages(messages);

            Console.WriteLine(formatedMessages);

            chatClient.UploadString(
                Configuration.Instance.Value.RocketChatWebhookUrl,
                JsonConvert.SerializeObject(new { text = formatedMessages }));
        }

        public string FormatMessages(IEnumerable<IMessage> outputMessages)
        {
            string message;
            var messages = outputMessages as IMessage[] ?? outputMessages.ToArray();
            if (messages.Any())
            {
                message = string.Join("\n", messages.Select(msg =>
                    msg.Message.AsRocketMessageTo(msg.Receipents).PrepareRocketLinkMessage(msg.LinkUrl, msg.LinkText)));
            }
            else
            {
                message = "Empty! :sunglasses:";
            }

            return message;
        }
    }
}