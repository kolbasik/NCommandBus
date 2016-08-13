using System.Collections.Generic;
using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Core
{
    public abstract class CommandBus : ICommandBus
    {
        public CommandBus()
        {
            CommandObservers = new List<ICommandObserver>();
            CommandValidators = new List<ICommandValidator> {DataAnnotationsCommandValidator.Instance};
        }

        public List<ICommandObserver> CommandObservers { get; }
        public List<ICommandValidator> CommandValidators { get; }

        public async Task<TResult> Send<TResult, TCommand>(TCommand command)
        {
            var context = new CommandContext(command);

            foreach (var commandObserver in CommandObservers)
                await commandObserver.PreExecute(context).ConfigureAwait(false);

            foreach (var commandValidator in CommandValidators)
                await commandValidator.Validate(context).ConfigureAwait(false);

            if (context.ValidationResults.Count > 0)
                throw new CommandBusValidationException(context.Command, context.ValidationResults);

            context.Result = await Execute<TResult, TCommand>(command).ConfigureAwait(false);

            foreach (var commandObserver in CommandObservers)
                await commandObserver.PostExecute(context).ConfigureAwait(false);

            return (TResult) context.Result;
        }

        protected abstract Task<TResult> Execute<TResult, TCommand>(TCommand command);
    }
}