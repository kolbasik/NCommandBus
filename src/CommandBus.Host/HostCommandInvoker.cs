using System;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Host
{
    public sealed class HostCommandInvoker : ICommandInvoker
    {
        private readonly IServiceProvider serviceProvider;

        /// <param name="serviceProvider">The <see cref="System.ComponentModel.Design.ServiceContainer" /> can be used as a native .net implementation.</param>
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