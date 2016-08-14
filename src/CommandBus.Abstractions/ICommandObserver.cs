using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface ICommandObserver
    {
        Task PreInvoke<TResult, TCommand>(CommandContext<TCommand> context);
        Task PostInvoke<TResult, TCommand>(CommandContext<TCommand> context);
    }
}