using System;
using System.Threading.Tasks;
using Thon.Hotels.FishBus;

namespace FishbusTests.MessageHandlers
{
    public class MessageA
    {

    }

    public class HandlerA : IHandleMessage<MessageA>
    {
        public Task Handle(MessageA message, Func<Task> markAsComplete)
        {
            throw new NotImplementedException();
        }
    }
}