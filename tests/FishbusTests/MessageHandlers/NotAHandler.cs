using System;
using System.Threading.Tasks;
using Thon.Hotels.FishBus;

namespace FishbusTests.MessageHandlers;

// does not inherit IHandleMessage<T>, so it is not a message handler
public class NotAHandler
{
    public Task<HandlerResult> Handle(MessageA message)
    {
        throw new NotImplementedException();
    }
}