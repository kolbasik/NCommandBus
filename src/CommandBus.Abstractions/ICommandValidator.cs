using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Core
{
    public interface ICommandValidator
    {
        Task Validate(CommandContext context);
    }
}