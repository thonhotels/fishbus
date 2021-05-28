using System;
using FishbusTests.MessageHandlers;
using Thon.Hotels.FishBus;
using Xunit;

namespace FishbusTests
{
    public class MessageBuilderTests
    {
        [Fact]
        public void MessageWithMessageIdAttributeUseCustomId()
        {
            var messageId = "messageId";
            var messageWithId = new MessageWithMessageId { Id = messageId };

            var message = MessageBuilder.BuildMessage(messageWithId);

            Assert.Equal(messageId, message.MessageId);
        }

        [Fact]
        public void MessageWithMessageIdAttributeUseCustomIdEvenWhenNull()
        {
            string messageId = null;
            var messageWithId = new MessageWithMessageId();

            var message = MessageBuilder.BuildMessage(messageWithId);

            Assert.Equal(messageId, message.MessageId);
        }

        [Fact]
        public void MessageWithMoreThanOneMessageIdAttributeThrowExcption()
        {
            var invalidMessage = new MessageWithTooManyMessageIds { Id = "id", AnothterId = "anotherId" };

            Assert.Throws<Exception>(() => MessageBuilder.BuildMessage(invalidMessage));
        }

        [Fact]
        public void MessageWithMoreThanOneMessageIdAttributeThrowExcptionEvenWithIdsSetToNull()
        {
            var invalidMessage = new MessageWithTooManyMessageIds();
            Assert.Throws<Exception>(() => MessageBuilder.BuildMessage(invalidMessage));
        }

        [Fact]
        public void MessageWithoutMessageIdAttributeIdNotSet()
        {
            var messageWithoutMessageId = new MessageWithoutMessageId();

            var message = MessageBuilder.BuildMessage(messageWithoutMessageId);

            Assert.Equal(null, message.MessageId);
        }

        [Fact]
        public void MessageWithoutCorrelationIdHasOneAssigned()
        {
            var message = new MessageWithMessageId();
            var msg = MessageBuilder.BuildMessage(message);

            Assert.True(Guid.TryParse(msg.ApplicationProperties["logCorrelationId"] as string, out _));
        }

        [Fact]
        public void ExistingCorrelationIdAreNotModified()
        {
            var expected = "correlationId";
            var messageWithId = new MessageWithMessageId();
            var msg = MessageBuilder.BuildMessage(messageWithId, expected);

            Assert.Equal(expected, msg.ApplicationProperties["logCorrelationId"] as string);
        }

        [Fact]
        public void MessageWithDelaySetsScheduledEnqueueTime()
        {
            var messageWithId = new MessageWithMessageId();
            var msg = MessageBuilder.BuildDelayedMessage(messageWithId, TimeSpan.FromDays(1), string.Empty);

            Assert.True(DateTime.UtcNow.AddHours(23) < msg.ScheduledEnqueueTime);
        }

        [Fact]
        public void MessageWithLabelAttributeUsesAttribute()
        {
            var messageWithAttribute = new MessageWithLabelAttribute();
            var msg = MessageBuilder.BuildMessage(messageWithAttribute);

            Assert.Equal("A.Custom.Message.Label", msg.Subject);
        }

        [Fact]
        public void MessageWithoutLabelAttributeUsesTypeFullName()
        {
            var messageWithoutAttribute = new MessageA();
            var msg = MessageBuilder.BuildMessage(messageWithoutAttribute);

            Assert.Equal(typeof(MessageA).FullName, msg.Subject);
        }

        [Fact]
        public void MessageWithTimeToLiveAttributeShouldMakeBuilderSetTimeToLiveAttributeOnBuiltMessage()
        {
            var timeToLive = new TimeSpan(6, 6, 6);
            var messageWithTimeToLiveAttribute = new ValidMessageWithTimeToLiveAttribute
            {
                TimeToLive = timeToLive
            };
            var msg = MessageBuilder.BuildMessage(messageWithTimeToLiveAttribute);

            Assert.Equal(timeToLive, msg.TimeToLive);
        }


        [Fact]
        public void MessageWithMoreThanOneTimeToLiveAttributeShouldMakeBuilderThrow()
        {
            var timeToLive = new TimeSpan(6, 6, 6);
            var messageWithToManyTimeToLiveAttributes = new InvalidMessageWithMoreThanOneTimeToLiveAttribute
            {
                TimeToLive = timeToLive,
                TimeToLive2 = timeToLive,
            };
            Assert.Throws<Exception>(() => MessageBuilder.BuildMessage(messageWithToManyTimeToLiveAttributes));
        }

        [Fact]
        public void MessageWithTimeToLiveAttributeButWrongTypeShouldMakeBuilderSetTimeToLiveToNullInBuiltMessage()
        {
            var messageWithTimeToLiveAttributeAsString = new InvalidMessageWithTimeToLiveAttributeAsString
            {
                TimeToLiveAsString = "2019-01-01"
            };
            var message = MessageBuilder.BuildMessage(messageWithTimeToLiveAttributeAsString);
            Assert.False(message.TimeToLive == null);
        }

        [Fact]
        public void MessageWithNoTimeToLiveAttributeShouldMakeBuilderSetTimeToLiveToNullInBuiltMessage()
        {
            var messageWithNoTimeToLiveAttribute = new MessageWithNoTimeToLiveAttribute();
            var message = MessageBuilder.BuildMessage(messageWithNoTimeToLiveAttribute);
            Assert.False(message.TimeToLive == null);
        }
    }
}
