using System;
using System.Threading.Tasks;

namespace Thon.Hotels.FishBus
{
    public interface IHandleMessage<T>
    {
        Task<bool> Handle(T message);
    }
}
