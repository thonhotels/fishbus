using System;
using System.Linq;
using System.Reflection;
using FishbusTests.MessageHandlers;
using Thon.Hotels.FishBus;
using Xunit;

namespace FishbusTests
{
    public class MessageAttributesTests
    {
        [Fact]
        public void MessageWithMessageIdAttributeUseCustomId()
        {
            var messageId = "messageId";
            var messageWithId = new MessageWithMessageId { Id = messageId };

            var value = MessageAttributes.GetMessageId(messageWithId);

            Assert.Equal(messageId, value);
        }

        [Fact]
        public void MessageWithMessageIdAttributeUseCustomIdEvenWhenNull()
        {
            string messageId = null;
            var messageWithId = new MessageWithMessageId();

            var value = MessageAttributes.GetMessageId(messageWithId);

            Assert.Equal(messageId, value);
        }

        [Fact]
        public void MessageWithMoreThanOneMessageIdAttributeThrowExcption()
        {
            var invalidMessage = new MessageWithTooManyMessageIds {Id = "id", AnothterId = "anotherId"};

            Assert.Throws<Exception>(() => MessageAttributes.GetMessageId(invalidMessage));
        }

        [Fact]
        public void MessageWithMoreThanOneMessageIdAttributeThrowExcptionEvenWithIdsSetToNull()
        {
            var invalidMessage = new MessageWithTooManyMessageIds();
            Assert.Throws<Exception>(() => MessageAttributes.GetMessageId(invalidMessage));
        }

        [Fact]
        public void MessageWithoutMessageIdAttributeIdNotSet()
        {
            var messageWithId = new MessageWithoutMessageId();

            var value = MessageAttributes.GetMessageId(messageWithId);

            Assert.Equal(string.Empty, value);
        }
    }
}
