using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Core
{
    public interface ICommandHandler<in TCommand>
    {
        Task<object> Handle(TCommand command);
    }
}