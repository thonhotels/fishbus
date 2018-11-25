using System;
using System.Threading.Tasks;
using Thon.Hotels.FishBus;

namespace FishbusTests.MessageHandlers
{
    public class MessageA
    {
        public string AProp1 { get; set; }
    }

    public class HandlerA : IHandleMessage<MessageA>
    {
        public virtual Task Handle(MessageA message, Func<Task> markAsComplete)
        {
            return Task.CompletedTask;
        }
    }
}