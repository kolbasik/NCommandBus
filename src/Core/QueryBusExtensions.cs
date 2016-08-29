using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Core
{
    public static class QueryBusExtensions
    {
        public static Task<TResult> Ask<TResult, TCommand>(this IQueryBus queryBus, TCommand command)
            where TCommand : class
            where TResult : class
        {
            return queryBus.Ask<TResult, TCommand>(command, CancellationToken.None);
        }
    }
}