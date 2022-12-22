using System.Threading.Tasks;

namespace Thon.Hotels.FishBus;

public interface IHandleMessage<T>
{
    Task<HandlerResult> Handle(T message);
}