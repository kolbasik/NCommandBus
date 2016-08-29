using System.Threading;
using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface ICommandBus
    {
        Task Tell<TCommand>(TCommand command, CancellationToken cancellationToken)
            where TCommand : class;
    }
}