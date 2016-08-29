using System.Threading;
using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface IMessageInvoker
    {
        Task Invoke<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : class;
        Task<TResult> Invoke<TResult, TQuery>(TQuery query, CancellationToken cancellationToken) where TQuery : class where TResult : class;
    }
}