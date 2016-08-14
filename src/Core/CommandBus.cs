using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Core
{
    public sealed class CommandBus : ICommandBus
    {
        private readonly ICommandInvoker commandInvoker;

        public CommandBus(ICommandInvoker commandInvoker)
        {
            if (commandInvoker == null) throw new ArgumentNullException(nameof(commandInvoker));
            this.commandInvoker = commandInvoker;
            CommandObservers = new List<ICommandObserver> {ValidationInterceptor.Instance};
            CommandValidators = new List<ICommandValidator> {ValidationInterceptor.Instance};
        }

        public List<ICommandObserver> CommandObservers { get; }
        public List<ICommandValidator> CommandValidators { get; }

        public async Task<TResult> Send<TResult, TCommand>(TCommand command, CancellationToken cancellationToken)
        {
            var context = new CommandContext<TCommand>(command);

            foreach (var commandValidator in CommandValidators)
                await commandValidator.Validate(context).ConfigureAwait(false);

            foreach (var commandObserver in CommandObservers)
                await commandObserver.PreInvoke<TResult, TCommand>(context).ConfigureAwait(false);

            context.Result = await commandInvoker.Invoke<TResult, TCommand>(context, cancellationToken).ConfigureAwait(false);

            foreach (var commandObserver in CommandObservers)
                await commandObserver.PostInvoke<TResult, TCommand>(context).ConfigureAwait(false);

            return (TResult) context.Result;
        }
    }
}