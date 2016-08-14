using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Core
{
    public sealed class CommandBus : ICommandBus
    {
        private readonly ICommandInvoker commandInvoker;

        public CommandBus(ICommandInvoker commandInvoker)
        {
            if (commandInvoker == null) throw new ArgumentNullException(nameof(commandInvoker));
            this.commandInvoker = commandInvoker;
            CommandObservers = new List<ICommandObserver>();
            CommandValidators = new List<ICommandValidator> {DataAnnotationsCommandValidator.Instance};
        }

        public List<ICommandObserver> CommandObservers { get; }
        public List<ICommandValidator> CommandValidators { get; }

        public Task<TResult> Send<TResult, TCommand>(TCommand command)
        {
            return Send<TResult, TCommand>(command, CancellationToken.None);
        }

        public async Task<TResult> Send<TResult, TCommand>(TCommand command, CancellationToken cancellationToken)
        {
            var context = new CommandContext(command);

            foreach (var commandObserver in CommandObservers)
                await commandObserver.PreExecute(context).ConfigureAwait(false);

            foreach (var commandValidator in CommandValidators)
                await commandValidator.Validate(context).ConfigureAwait(false);

            if (context.ValidationResults.Count > 0)
                throw new CommandBusValidationException(context.Command, context.ValidationResults);

            context.Result = await commandInvoker.Invoke<TResult, TCommand>(command, cancellationToken).ConfigureAwait(false);

            foreach (var commandObserver in CommandObservers)
                await commandObserver.PostExecute(context).ConfigureAwait(false);

            return (TResult) context.Result;
        }
    }
}