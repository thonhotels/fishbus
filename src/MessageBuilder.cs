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
        internal static Message BuildDelayedMessage<T>(T message, TimeSpan timeSpan, string correlationId)
        {
            var msg = BuildMessage(message, correlationId);
            msg.ScheduledEnqueueTimeUtc = DateTime.UtcNow.Add(timeSpan);
            return msg;
        }

        internal static Message BuildScheduledMessage<T>(T message, DateTime time, string correlationId)
        {
            var msg = BuildMessage(message, correlationId);
            msg.ScheduledEnqueueTimeUtc = time;
            return msg;
        }

        internal static Message BuildMessage<T>(T message) => BuildMessage(message, string.Empty);

        internal static Message BuildMessage<T>(T message, string correlationId)
        {
            var id = GetMessageId(message);
            var label = GetMessageLabel(message);
            var timeToLive = GetTimeToLive(message);

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)))
            {
                Label = label
            };

            if (!string.IsNullOrWhiteSpace(id))
                msg.MessageId = id;

            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            if (timeToLive.HasValue)
            {
                msg.TimeToLive = timeToLive.Value;
            }
            msg.UserProperties.Add("logCorrelationId", correlationId);

            return msg;
        }


        private static string GetMessageId<T>(T message)
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

        private static string GetMessageLabel<T>(T message)
        {
            var label = message.GetType().GetCustomAttribute<MessageLabelAttribute>()?.Label;

            if (string.IsNullOrWhiteSpace(label))
                return message.GetType().FullName;

            return label;
        }

        private static TimeSpan? GetTimeToLive<T>(T message)
        {
            var timeToLiveAttribute = message.GetType().GetProperties()
                .Select(pi => new
                {
                    Property = pi,
                    Attribute = pi.GetCustomAttribute(typeof(TimeToLiveAttribute), true) as TimeToLiveAttribute
                })
                .Where(x => x.Attribute != null)
                .ToList();

            if (!timeToLiveAttribute.Any())
                return null;

            if (timeToLiveAttribute.Count > 1)
                throw new Exception($"At most one property of {message.GetType().Name} can be marked with the {nameof(TimeToLiveAttribute)} attribute");

            return timeToLiveAttribute.FirstOrDefault()?.Property.GetValue(message, null) as TimeSpan?;
        }
    }
}