using System;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Host
{
    public sealed class InProcessMessageInvoker : IMessageInvoker
    {
        private readonly IServiceProvider serviceProvider;

        /// <param name="serviceProvider">The <see cref="System.ComponentModel.Design.ServiceContainer" /> can be used as a native .net implementation.</param>
        public InProcessMessageInvoker(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
            this.serviceProvider = serviceProvider;
        }

        public Task Invoke<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : class
        {
            var commandHandlerType = typeof(ICommandHandler<TCommand>);
            var commandHandler = serviceProvider.GetService(commandHandlerType) as ICommandHandler<TCommand>;
            if (commandHandler == null)
            {
                throw new InvalidOperationException($"Could not resolve the {commandHandlerType.FullName} type.");
            }
            return commandHandler.Handle(command, cancellationToken);
        }

        public Task<TResult> Invoke<TResult, TQuery>(TQuery query, CancellationToken cancellationToken) where TResult : class where TQuery : class
        {
            var queryHandlerType = typeof(IQueryHandler<TQuery, TResult>);
            var queryHandler = serviceProvider.GetService(queryHandlerType) as IQueryHandler<TQuery, TResult>;
            if (queryHandler == null)
            {
                throw new InvalidOperationException($"Could not resolve the {queryHandlerType.FullName} type.");
            }
            return queryHandler.Handle(query, cancellationToken);
        }
    }
}