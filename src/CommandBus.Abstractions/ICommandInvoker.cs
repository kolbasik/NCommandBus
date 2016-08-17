using System.Threading;
using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface ICommandInvoker
    {
        Task<TResult> Invoke<TResult, TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : class where TResult : class;
    }
}