using System;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Core;

namespace kolbasik.NCommandBus.Host
{
    public sealed class HostCommandBus : CommandBus
    {
        private readonly IServiceProvider serviceProvider;

        public HostCommandBus(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
            this.serviceProvider = serviceProvider;
        }

        protected override async Task<TResult> Execute<TResult, TCommand>(TCommand command)
        {
            var commandHandlerType = typeof(ICommandHandler<TCommand>);
            var commandHandler = serviceProvider.GetService(commandHandlerType) as ICommandHandler<TCommand>;
            if (commandHandler == null)
            {
                throw new InvalidOperationException($"Could not resolve the {commandHandlerType.FullName} type.");
            }
            var result = await commandHandler.Handle(command).ConfigureAwait(false);
            return (TResult) result;
        }
    }
}