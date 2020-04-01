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
        class AttachmentFields
        {
            public bool @short;
            public string title;
            public string value;
        }

        class Attachment
        {
            public AttachmentFields[] fields = new AttachmentFields[] { };
            public string author_name;
        }

        public void SendMessages(IEnumerable<IMessage> messages)
        {
            var chatClient = new WebClient();
            var formattedMessages = FormatMessages(messages);

            Console.WriteLine(formattedMessages);

            chatClient.UploadString(
                Configuration.Instance.Value.RocketChatWebhookUrl,
                JsonConvert.SerializeObject(new
                    {
                        text = formattedMessages,
                        attachments = new Attachment[] { }
                    }));
        }

        public string FormatMessages(IEnumerable<IMessage> outputMessages)
        {
            string message;

            var messages = outputMessages as IMessage[] ?? outputMessages.ToArray();
            if (messages.Any())
            {
                message = messages.GroupBy(m => m.Request.ProjectDetails.ShortName)
                    .SelectMany(projectMessages =>
                    {
                        return new[] { $"*{projectMessages.Key}*" }.Concat(
                            projectMessages.GroupBy(m => m.LinkUrl)
                                .Select(groupedMessages =>
                                {
                                    return new[] { $"• [{groupedMessages.First().LinkText}]({groupedMessages.Key})".Indent(1) }
                                    .Concat(groupedMessages.Select(msg => ("• " + msg.Message.AsRocketMessageTo(msg.Recipients)).Indent(3)))
                                    .ToSingleMessage();
                                })
                         );
                    }).ToSingleMessage();
            }
            else
            {
                message = "Empty! :sunglasses:";
            }

            return message;
        }
    }
}