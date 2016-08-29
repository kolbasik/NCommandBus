using System;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using MassTransit;

namespace kolbasik.NCommandBus.MassTransit
{
    public sealed class MassTransitMessageInvoker : IMessageInvoker
    {
        private readonly IBusControl busControl;
        private readonly Uri address;
        private readonly TimeSpan timeout;

        public MassTransitMessageInvoker(IBusControl busControl, Uri address, TimeSpan timeout)
        {
            if (busControl == null)
                throw new ArgumentNullException(nameof(busControl));
            this.busControl = busControl;
            this.address = address;
            this.timeout = timeout;
        }

        public Task Invoke<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : class
        {
            return busControl.Publish<TCommand>(command, cancellationToken);
        }

        public async Task<TResult> Invoke<TResult, TQuery>(TQuery query, CancellationToken cancellationToken)
            where TQuery : class
            where TResult : class
        {
            var client = busControl.CreateRequestClient<TQuery, TResult>(address, timeout);
            var result = await client.Request(query, cancellationToken).ConfigureAwait(false);
            return result;
        }
    }
}
