using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Arek.Contracts;
using Newtonsoft.Json;

namespace Arek.RocketChat
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
                JsonConvert.SerializeObject(new { text = formatedMessages, attachments = new List<string>() }));
        }

        public string FormatMessages(IEnumerable<IMessage> outputMessages)
        {
            string message;

            var messages = outputMessages as IMessage[] ?? outputMessages.ToArray();
            if (messages.Any())
            {
                var allFormattedMessages = messages.Select(msg =>
                                    msg.Message.AsRocketMessageTo(msg.Receipents).PrepareRocketLinkMessage(msg.LinkUrl, msg.LinkText))
                                    .Distinct();

                message = string.Join("\n", allFormattedMessages);
            }
            else
            {
                message = "Empty! :sunglasses:";
            }

            return message;
        }
    }
}