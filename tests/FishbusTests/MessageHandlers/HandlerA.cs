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
        public virtual Task<bool> Handle(MessageA message)
        {
            return Task.FromResult(true);
        }
    }
}