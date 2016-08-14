using System.Threading;
using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Core
{
    public interface ICommandInvoker
    {
        Task<TResult> Invoke<TResult, TCommand>(TCommand command, CancellationToken cancellationToken);
    }
}