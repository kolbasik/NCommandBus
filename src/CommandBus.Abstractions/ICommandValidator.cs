using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface ICommandValidator
    {
        Task Validate<TCommand, TResult>(CommandContext<TCommand, TResult> context);
    }
}