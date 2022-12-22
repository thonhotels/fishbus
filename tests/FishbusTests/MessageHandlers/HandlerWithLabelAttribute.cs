using System;
using System.Threading.Tasks;
using Thon.Hotels.FishBus;

namespace FishbusTests.MessageHandlers;

[MessageSubject("A.Custom.Message.Label")]
public class MessageWithLabelAttribute
{

}

public class HandlerWithLabelAttribute : IHandleMessage<MessageWithLabelAttribute>
{
    public Task<HandlerResult> Handle(MessageWithLabelAttribute message)
    {
        throw new NotImplementedException();
    }
}