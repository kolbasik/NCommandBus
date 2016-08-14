using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Core
{
    public interface ICommandHandler<in TCommand, TResult>
    {
        Task<TResult> Handle(TCommand command);
    }
}