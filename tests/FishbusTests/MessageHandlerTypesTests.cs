using System;
using System.Collections.Generic;
using System.Reflection;
using FishbusTests.MessageHandlers;
using FluentAssertions;
using Thon.Hotels.FishBus;
using Xunit;

namespace FishbusTests;

public class MessageHandlerTypesTests
{
    [Fact]
    public void GetAll_MessageHandlers_ReturnsEachHandlerExactlyOnce()
    {
        var expectedHandlers = new List<Type>
        {
            typeof(DuoHandler),
            typeof(HandlerA),
            typeof(HandlerA1),
            typeof(HandlerA2),
            typeof(HandlerB),
            typeof(HandlerWithLabelAttribute)
        };

        var messageHandlers = MessageHandlerTypes.GetAll(Assembly.GetExecutingAssembly());

        messageHandlers.Should().BeEquivalentTo(expectedHandlers);
    }
}