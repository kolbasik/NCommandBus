using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Core
{
    public interface ICommandObserver
    {
        Task PreExecute<TCommand, TResult>(CommandContext<TCommand, TResult> context);
        Task PostExecute<TCommand, TResult>(CommandContext<TCommand, TResult> context);
    }
}