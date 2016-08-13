using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Core
{
    public interface ICommandObserver
    {
        Task PreExecute(CommandContext context);
        Task PostExecute(CommandContext context);
    }
}