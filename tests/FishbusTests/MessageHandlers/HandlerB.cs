using System;
using System.Threading.Tasks;
using Thon.Hotels.FishBus;

namespace FishbusTests.MessageHandlers
{
    public class MessageB
    {

    }

    public class HandlerB : IHandleMessage<MessageB>
    {
        public Task Handle(MessageB message, Func<Task> markAsComplete)
        {
            throw new NotImplementedException();
        }
    }
}