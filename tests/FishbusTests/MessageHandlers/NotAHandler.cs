using System;
using System.Threading.Tasks;

namespace FishbusTests.MessageHandlers
{
    // does not inherit IHandleMessage<T>, so it is not a message handler
    public class NotAHandler
    {
        public Task Handle(MessageA message, Func<Task> markAsComplete)
        {
            throw new NotImplementedException();
        }
    }
}