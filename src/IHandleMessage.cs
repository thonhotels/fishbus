using System;
using System.Threading.Tasks;

namespace Thon.Hotels.FishBus
{
    public interface IHandleMessage<T>
    {
        Task Handle(T message, Func<Task> markAsComplete);
    }
}
