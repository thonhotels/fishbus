using System;
using System.Threading.Tasks;
using Thon.Hotels.FishBus;

namespace FishbusTests.MessageHandlers;

public class MessageA
{
    public string AProp1 { get; set; }
}

public class HandlerA : IHandleMessage<MessageA>
{
    public virtual Task<HandlerResult> Handle(MessageA message)
    {
        return Task.FromResult(HandlerResult.Success());
    }
}

public class HandlerA1 : IHandleMessage<MessageA>
{
    public virtual Task<HandlerResult> Handle(MessageA message)
    {
        throw new NotImplementedException();
    }
}

public class HandlerA2 : IHandleMessage<MessageA>
{
    public virtual Task<HandlerResult> Handle(MessageA message)
    {
        throw new NotImplementedException();
    }
}