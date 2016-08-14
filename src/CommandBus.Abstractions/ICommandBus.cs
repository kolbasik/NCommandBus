using System.Threading;
using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface ICommandBus
    {
        Task<TResult> Send<TResult, TCommand>(TCommand command, CancellationToken cancellationToken);
    }
}