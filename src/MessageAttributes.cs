using System;
using System.Linq;
using System.Reflection;

namespace Thon.Hotels.FishBus
{
    public static class MessageAttributes
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
    }
}