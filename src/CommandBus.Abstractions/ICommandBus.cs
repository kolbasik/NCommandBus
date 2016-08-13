using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Core
{
    public interface ICommandBus
    {
        Task<TResult> Send<TResult, TCommand>(TCommand command);
    }
}