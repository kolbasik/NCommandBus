using System;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Core;

namespace kolbasik.NCommandBus.Host
{
    public sealed class HostCommandInvoker : ICommandInvoker
    {
        private readonly IServiceProvider serviceProvider;

        public HostCommandInvoker(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
            this.serviceProvider = serviceProvider;
        }

        public Task<TResult> Invoke<TResult, TCommand>(TCommand command, CancellationToken cancellationToken)
        {
            var commandHandlerType = typeof(ICommandHandler<TCommand, TResult>);
            var commandHandler = serviceProvider.GetService(commandHandlerType) as ICommandHandler<TCommand, TResult>;
            if (commandHandler == null)
            {
                throw new InvalidOperationException($"Could not resolve the {commandHandlerType.FullName} type.");
            }
            return commandHandler.Handle(command);
        }
    }
}