using System.Threading;
using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface IQueryBus
    {
        Task<TResult> Ask<TResult, TQuery>(TQuery query, CancellationToken cancellationToken)
            where TQuery : class
            where TResult : class;
    }
}