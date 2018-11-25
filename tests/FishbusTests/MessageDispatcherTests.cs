using System;
using System.Collections.Generic;
using FishbusTests.MessageHandlers;
using Thon.Hotels.FishBus;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Azure.ServiceBus.Core;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace FishbusTests
{
    public class MessageDispatcherTests
    {
        [Fact]
        public async Task WhenProcessMessageIsCalledWithMessageATheMessageAHandlerIsCalled()
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
            var scopeFactory = A.Fake<IServiceScopeFactory>();
            var scope = A.Fake<IServiceScope>();
            var sp = A.Fake<IServiceProvider>();
            var handler = A.Fake<HandlerA>();

            A.CallTo(() => sp.GetService(A<Type>.That.IsEqualTo(typeof(HandlerA)))).Returns(handler);

            A.CallTo(() => scopeFactory.CreateScope()).Returns(scope);
            A.CallTo(() => scope.ServiceProvider).Returns(sp);

            var client = A.Fake<IReceiverClient>();
            var registry = new MessageHandlerRegistry(MessageHandlerTypes);
            var sut = new MessageDispatcher(scopeFactory, client, registry);

            await sut.ProcessMessage(typeof(MessageA).FullName, "{aProp1: \"hello\"}", () => Task.CompletedTask);

            A.CallTo(() => handler.Handle(A<MessageA>.That.Matches(m => m.AProp1 == "hello"), A<Func<Task>>.Ignored))
                .MustHaveHappenedOnceExactly();            
        }
    }
}
