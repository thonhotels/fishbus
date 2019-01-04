﻿using System;
using System.Linq;
using System.Reflection;
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

            var value = MessageBuilder.GetMessageId(messageWithId);

            Assert.Equal(messageId, value);
        }

        [Fact]
        public void MessageWithMessageIdAttributeUseCustomIdEvenWhenNull()
        {
            string messageId = null;
            var messageWithId = new MessageWithMessageId();

            var value = MessageBuilder.GetMessageId(messageWithId);

            Assert.Equal(messageId, value);
        }

        [Fact]
        public void MessageWithMoreThanOneMessageIdAttributeThrowExcption()
        {
            var invalidMessage = new MessageWithTooManyMessageIds { Id = "id", AnothterId = "anotherId" };

            Assert.Throws<Exception>(() => MessageBuilder.GetMessageId(invalidMessage));
        }

        [Fact]
        public void MessageWithMoreThanOneMessageIdAttributeThrowExcptionEvenWithIdsSetToNull()
        {
            var invalidMessage = new MessageWithTooManyMessageIds();
            Assert.Throws<Exception>(() => MessageBuilder.GetMessageId(invalidMessage));
        }

        [Fact]
        public void MessageWithoutMessageIdAttributeIdNotSet()
        {
            var messageWithoutMessageId = new MessageWithoutMessageId();

            var value = MessageBuilder.GetMessageId(messageWithoutMessageId);

            Assert.Equal(string.Empty, value);
        }

        [Fact]
        public void MessageWithoutCorralationIdHasOneAssigned()
        {
            var message = new MessageWithMessageId();
            var msg = MessageBuilder.BuildMessage(message);

            Assert.True(Guid.TryParse(msg.CorrelationId, out _));
        }

        [Fact]
        public void ExistingCorralationIdAreNotModified()
        {
            var expected = "correlationId";
            var messageWithId = new MessageWithMessageId();
            var msg = MessageBuilder.BuildMessage(messageWithId, expected);

            Assert.Equal(expected, msg.CorrelationId);
        }
    }
}
