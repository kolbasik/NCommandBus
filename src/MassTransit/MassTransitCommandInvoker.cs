using System;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using MassTransit;

namespace kolbasik.NCommandBus.MassTransit
{
    public sealed class MassTransitCommandInvoker : ICommandInvoker
    {
        private readonly IBusControl busControl;
        private readonly Uri address;
        private readonly TimeSpan timeout;

        public MassTransitCommandInvoker(IBusControl busControl, Uri address, TimeSpan timeout)
        {
            if (busControl == null)
                throw new ArgumentNullException(nameof(busControl));
            this.busControl = busControl;
            this.address = address;
            this.timeout = timeout;
        }

        public async Task<TResult> Invoke<TResult, TCommand>(TCommand command, CancellationToken cancellationToken)
            where TCommand : class
            where TResult : class
        {
            var client = busControl.CreateRequestClient<TCommand, TResult>(address, timeout);
            var result = await client.Request(command).ConfigureAwait(false);
            return result;
        }
    }
}
