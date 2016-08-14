using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface ICommandValidator
    {
        Task Validate<TCommand>(CommandContext<TCommand> context);
    }
}