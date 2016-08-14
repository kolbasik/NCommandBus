using System.Threading;
using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface ICommandInvoker
    {
        Task<TResult> Invoke<TResult, TCommand>(CommandContext<TCommand> context, CancellationToken cancellationToken);
    }
}