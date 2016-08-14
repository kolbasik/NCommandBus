using System.Threading;
using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Core
{
    public static class CommandBusExtensions
    {
        public static Task<TResult> Send<TResult, TCommand>(this CommandBus commandBus, TCommand command)
        {
            return commandBus.Send<TResult, TCommand>(command, CancellationToken.None);
        }
    }
}