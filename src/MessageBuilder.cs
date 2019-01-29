using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace Thon.Hotels.FishBus
{
    public static class MessageBuilder
    {
        public static string GetMessageId<T>(T message)
        {
            var messageIdAttributes = message.GetType().GetProperties()
                .Select(pi => new
                {
                    Property = pi,
                    Attribute = pi.GetCustomAttribute(typeof(MessageIdAttribute), true) as MessageIdAttribute
                })
                .Where(x => x.Attribute != null)
                .ToList();

            if (!messageIdAttributes.Any())
                return string.Empty;

            if (messageIdAttributes.Count > 1)
                throw new Exception($"At most one property of {message.GetType().Name} can be marked with the {nameof(MessageIdAttribute)} attribute");

            return messageIdAttributes.FirstOrDefault()?.Property.GetValue(message, null) as string;
        }

        public static string GetMessageLabel<T>(T message)
        {
            var label = message.GetType().GetCustomAttribute<MessageLabelAttribute>()?.Label;

            if (string.IsNullOrWhiteSpace(label))
                throw new Exception($"Label must be specified on {message.GetType().Name} using MessageLabelAttribute");

            return label;
        }

        public static Message BuildMessage<T>(T message, string correlationId)
        {
            var id = GetMessageId(message);
            var label = GetMessageLabel(message);

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)))
            {
                Label = label
            };

            if (!string.IsNullOrWhiteSpace(id))
                msg.MessageId = id;

            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            msg.UserProperties.Add("logCorrelationId", correlationId);

            return msg;
        }

        public static Message BuildMessage<T>(T message) => BuildMessage(message, string.Empty);
    }
}