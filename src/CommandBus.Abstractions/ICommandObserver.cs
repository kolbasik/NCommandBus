using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface ICommandObserver
    {
        Task PreInvoke<TCommand, TResult>(CommandContext<TCommand, TResult> context);
        Task PostInvoke<TCommand, TResult>(CommandContext<TCommand, TResult> context);
    }
}