using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Core
{
    public interface ICommandValidator
    {
        Task Validate<TCommand, TResult>(CommandContext<TCommand, TResult> context);
    }
}