using System;
using System.Threading.Tasks;
using Thon.Hotels.FishBus;

namespace FishbusTests.MessageHandlers;

public class DuoHandler : IHandleMessage<MessageA>, IHandleMessage<MessageB>
{
    public virtual Task<HandlerResult> Handle(MessageA message)
    {
        throw new NotImplementedException();
    }

    public virtual Task<HandlerResult> Handle(MessageB message)
    {
        throw new NotImplementedException();
    }
}