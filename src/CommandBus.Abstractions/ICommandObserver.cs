using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface ICommandObserver
    {
        Task PreExecute<TCommand, TResult>(CommandContext<TCommand, TResult> context);
        Task PostExecute<TCommand, TResult>(CommandContext<TCommand, TResult> context);
    }
}