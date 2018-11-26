using System;
using System.Collections.Generic;
using FishbusTests.MessageHandlers;
using Thon.Hotels.FishBus;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace FishbusTests
{
    public class MessageHandlerRegistryTests
    {
        [Fact]
        public void GetHandlersReturnHandlerForGivenMessageType()
        {
            IEnumerable<Type> MessageHandlerTypes()
            {
                return new [] 
                    {
                        typeof(HandlerA),
                        typeof(HandlerB),
                        typeof(NotAHandler)
                    };
            }
            var sut = new MessageHandlerRegistry(MessageHandlerTypes);
            var sp = A.Fake<IServiceProvider>();
            A.CallTo(() => sp.GetService(A<Type>.That.IsEqualTo(typeof(HandlerA)))).Returns(new HandlerA());
            var scope = A.Fake<IServiceScope>();
            A.CallTo(() => scope.ServiceProvider).Returns(sp);

            var handlers = sut.GetHandlers(typeof(MessageA), scope).ToList();
            Assert.NotEmpty(handlers);
            Assert.Single(handlers);
            Assert.Single(handlers, h => h is HandlerA);
        }

        [Fact]
        public void GetHandlersDoesNotReturnHandlerForGivenMessageTypeIfHandlerIsntARealHandler()
        {
            IEnumerable<Type> MessageHandlerTypes()
            {
                return new [] 
                    {
                        typeof(HandlerB),
                        typeof(NotAHandler)
                    };
            }
            var sut = new MessageHandlerRegistry(MessageHandlerTypes);
            var sp = A.Fake<IServiceProvider>();
            A.CallTo(() => sp.GetService(A<Type>.That.IsEqualTo(typeof(NotAHandler)))).Returns(new NotAHandler());
            var scope = A.Fake<IServiceScope>();
            A.CallTo(() => scope.ServiceProvider).Returns(sp);

            var handlers = sut.GetHandlers(typeof(MessageA), scope).ToList();
            Assert.Empty(handlers);
        }

        [Fact]
        public void GetMessageTypeByNameReturnsTypeOfRegisteredMessage()
        {
            IEnumerable<Type> MessageHandlerTypes()
            {
                return new [] 
                    {
                        typeof(HandlerA),
                        typeof(HandlerB),
                        typeof(NotAHandler)
                    };
            }
            var sut = new MessageHandlerRegistry(MessageHandlerTypes);

            var messageType = sut.GetMessageTypeByName(typeof(MessageA).FullName);
            Assert.NotNull(messageType);
            Assert.Equal(typeof(MessageA), messageType);
        }

        [Fact]
        public void GetMessageTypeByNameReturnsTypeMatchingMessageLabel()
        {
            IEnumerable<Type> MessageHandlerTypes()
            {
                return new[]
                {
                    typeof(HandlerA),
                    typeof(HandlerB),
                    typeof(NotAHandler),
                    typeof(HandlerWithLabelAttribute)
                };
            }
            var sut = new MessageHandlerRegistry(MessageHandlerTypes);

            var messageType = sut.GetMessageTypeByName("A.Custom.Message.Label");
            Assert.NotNull(messageType);
            Assert.Equal(typeof(MessageWithLabelAttribute), messageType);
        }
    }
}
