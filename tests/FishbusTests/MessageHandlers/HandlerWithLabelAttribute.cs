using System;
using System.Threading.Tasks;
using Thon.Hotels.FishBus;

namespace FishbusTests.MessageHandlers
{
    [MessageLabel("A.Custom.Message.Label")]
    public class MessageWithLabelAttribute
    {

    }

    public class HandlerWithLabelAttribute : IHandleMessage<MessageWithLabelAttribute>
    {
        public Task Handle(MessageWithLabelAttribute message, Func<Task> markAsComplete)
        {
            throw new NotImplementedException();
        }
    }
}