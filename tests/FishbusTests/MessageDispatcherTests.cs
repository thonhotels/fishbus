using System;
using System.Collections.Generic;
using FishbusTests.MessageHandlers;
using Thon.Hotels.FishBus;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace FishbusTests;

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

        A.CallTo(() => handler.Handle(A<MessageA>._)).Returns(Task.FromResult(HandlerResult.Success()));

        A.CallTo(() => sp.GetService(A<Type>.That.IsEqualTo(typeof(HandlerA)))).Returns(handler);

        A.CallTo(() => scopeFactory.CreateScope()).Returns(scope);
        A.CallTo(() => scope.ServiceProvider).Returns(sp);

        // var client = A.Fake<IReceiverClient>();
        var registry = new MessageHandlerRegistry(MessageHandlerTypes);
        var sut = new MessageDispatcher(scopeFactory, (null, null), registry, new LogCorrelationHandler(false), DefaultJsonOptions.Get);

        await sut.ProcessMessage(typeof(MessageA).FullName, "{\"aProp1\": \"hello\"}", () => Task.CompletedTask, m => Task.CompletedTask);

        A.CallTo(() => handler.Handle(A<MessageA>.That.Matches(m => m.AProp1 == "hello")))
            .MustHaveHappenedOnceExactly();            
    }

    [Fact]
    public async Task WhenProcessMessageMarksMessageWithNoHandlersComplete()
    {
        IEnumerable<Type> MessageHandlerTypes()
        {
            return new [] 
            {
                typeof(HandlerB),
            };
        }
        var scopeFactory = A.Fake<IServiceScopeFactory>();
        var scope = A.Fake<IServiceScope>();
        var sp = A.Fake<IServiceProvider>();
        var handler = A.Fake<HandlerA>();

        A.CallTo(() => sp.GetService(A<Type>.That.IsEqualTo(typeof(HandlerA)))).Returns(handler);

        A.CallTo(() => scopeFactory.CreateScope()).Returns(scope);
        A.CallTo(() => scope.ServiceProvider).Returns(sp);

        var registry = new MessageHandlerRegistry(MessageHandlerTypes);
        var sut = new MessageDispatcher(scopeFactory, (null, null), registry, new LogCorrelationHandler(false), DefaultJsonOptions.Get);

        var isCompleted = false;
        await sut.ProcessMessage(typeof(MessageA).FullName, "{\"aProp1\": \"hello\"}", () => 
        {
            isCompleted = true;
            return Task.CompletedTask;
        }, m => Task.CompletedTask);

        Assert.True(isCompleted, "the markComplete callback was not called");        
    }

    [Fact]
    public async Task WhenOneHandlerReturnsAbortMessageIsAborted()
    {
        IEnumerable<Type> MessageHandlerTypes()
        {
            return new [] 
            {
                typeof(HandlerA1),
                typeof(HandlerA2)
            };
        }
        var scopeFactory = A.Fake<IServiceScopeFactory>();
        var scope = A.Fake<IServiceScope>();
        var sp = A.Fake<IServiceProvider>();
        var successHandler = A.Fake<IHandleMessage<MessageA>>();
        var abortHandler = A.Fake<IHandleMessage<MessageA>>();

        A.CallTo(() => sp.GetService(A<Type>.That.IsEqualTo(typeof(HandlerA1)))).Returns(successHandler);
        A.CallTo(() => sp.GetService(A<Type>.That.IsEqualTo(typeof(HandlerA2)))).Returns(abortHandler);

        A.CallTo(() => successHandler.Handle(A<MessageA>._)).Returns(Task.FromResult(HandlerResult.Success()));
        A.CallTo(() => abortHandler.Handle(A<MessageA>._)).Returns(Task.FromResult(HandlerResult.Abort("something bad happened")));

        A.CallTo(() => scopeFactory.CreateScope()).Returns(scope);
        A.CallTo(() => scope.ServiceProvider).Returns(sp);

        var registry = new MessageHandlerRegistry(MessageHandlerTypes);
        var sut = new MessageDispatcher(scopeFactory, (null, null), registry, new LogCorrelationHandler(false), DefaultJsonOptions.Get);

        var isCompleted = false;
        var isAborted = false;
        var message = "";
        await sut.ProcessMessage(typeof(MessageA).FullName, "{\"aProp1\": \"hello\"}", () => 
        {
            isCompleted = true;
            return Task.CompletedTask;
        }, m => 
        {
            message = m;
            isAborted = true;
            return Task.CompletedTask;
        });

        Assert.False(isCompleted, "the markComplete callback was called");  
        Assert.True(isAborted, "the abort callback was not called");   
        Assert.Equal("something bad happened", message);   
    }

    [Fact]
    public async Task WhenOneHandlerReturnsFailMarkCompleteIsNotCalled()
    {
        IEnumerable<Type> MessageHandlerTypes()
        {
            return new [] 
            {
                typeof(HandlerA1),
                typeof(HandlerA2)
            };
        }
        var scopeFactory = A.Fake<IServiceScopeFactory>();
        var scope = A.Fake<IServiceScope>();
        var sp = A.Fake<IServiceProvider>();
        var successHandler = A.Fake<IHandleMessage<MessageA>>();
        var failHandler = A.Fake<IHandleMessage<MessageA>>();

        A.CallTo(() => sp.GetService(A<Type>.That.IsEqualTo(typeof(HandlerA1)))).Returns(successHandler);
        A.CallTo(() => sp.GetService(A<Type>.That.IsEqualTo(typeof(HandlerA2)))).Returns(failHandler);

        A.CallTo(() => successHandler.Handle(A<MessageA>._)).Returns(Task.FromResult(HandlerResult.Success()));
        A.CallTo(() => failHandler.Handle(A<MessageA>._)).Returns(Task.FromResult(HandlerResult.Failed()));

        A.CallTo(() => scopeFactory.CreateScope()).Returns(scope);
        A.CallTo(() => scope.ServiceProvider).Returns(sp);

        var registry = new MessageHandlerRegistry(MessageHandlerTypes);
        var sut = new MessageDispatcher(scopeFactory, (null, null), registry, new LogCorrelationHandler(false), DefaultJsonOptions.Get);

        var isCompleted = false;
        var isAborted = false;
        var message = "";
        await sut.ProcessMessage(typeof(MessageA).FullName, "{\"aProp1\": \"hello\"}", () => 
        {
            isCompleted = true;
            return Task.CompletedTask;
        }, m => 
        {
            message = m;
            isAborted = true;
            return Task.CompletedTask;
        });

        Assert.False(isCompleted, "the markComplete callback was called");  
        Assert.False(isAborted, "the abort callback was not called");   
        Assert.Equal("", message);   
    }

    [Fact]
    public async Task WhenHandlerHandlesAandB_And_BMessage_Received_Only_B_Method_is_Called()
    {
        IEnumerable<Type> MessageHandlerTypes()
        {
            return new [] 
            {
                typeof(DuoHandler)
            };
        }
        var scopeFactory = A.Fake<IServiceScopeFactory>();
        var scope = A.Fake<IServiceScope>();
        var sp = A.Fake<IServiceProvider>();
        var duoHandler = A.Fake<DuoHandler>();

        A.CallTo(() => sp.GetService(A<Type>.That.IsEqualTo(typeof(DuoHandler)))).Returns(duoHandler);

        A.CallTo(() => duoHandler.Handle(A<MessageA>._)).Returns(Task.FromResult(HandlerResult.Success()));
        A.CallTo(() => duoHandler.Handle(A<MessageB>._)).Returns(Task.FromResult(HandlerResult.Success()));

        A.CallTo(() => scopeFactory.CreateScope()).Returns(scope);
        A.CallTo(() => scope.ServiceProvider).Returns(sp);

        var registry = new MessageHandlerRegistry(MessageHandlerTypes);
        var sut = new MessageDispatcher(scopeFactory, (null, null), registry, new LogCorrelationHandler(false), DefaultJsonOptions.Get);

        var isCompleted = false;
        var isAborted = false;
        var message = "";
        await sut.ProcessMessage(typeof(MessageB).FullName, "{\"aProp1\": \"hello\"}", () => 
        {
            isCompleted = true;
            return Task.CompletedTask;
        }, m => 
        {
            message = m;
            isAborted = true;
            return Task.CompletedTask;
        });

        Assert.True(isCompleted, "the markComplete callback was called");  
        Assert.False(isAborted, "the abort callback was not called");   
        Assert.Equal("", message);  
        A.CallTo(() => duoHandler.Handle(A<MessageA>._)).MustNotHaveHappened();
        A.CallTo(() => duoHandler.Handle(A<MessageB>._)).MustHaveHappenedOnceExactly(); 
    }
}