using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface ICommandObserver
    {
        Task PreInvoke<TResult, TCommand>(CommandContext<TCommand, TResult> context);
        Task PostInvoke<TResult, TCommand>(CommandContext<TCommand, TResult> context);
    }
}