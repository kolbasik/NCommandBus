using System.Threading;
using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface ICommandHandler<in TCommand>
    {
        Task Handle(TCommand command, CancellationToken cancellationToken);
    }
}