using System;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using MassTransit;

namespace kolbasik.NCommandBus.MassTransit
{
    public sealed class CommandBusConsumer<TCommand, TResult> : IConsumer<TCommand> where TCommand : class where TResult : class
    {
        private readonly ICommandBus commandBus;

        public CommandBusConsumer(ICommandBus commandBus)
        {
            if (commandBus == null)
                throw new ArgumentNullException(nameof(commandBus));
            this.commandBus = commandBus;
        }

        public async Task Consume(ConsumeContext<TCommand> context)
        {
            var result = await commandBus.Send<TResult, TCommand>(context.Message, context.CancellationToken).ConfigureAwait(false);
            await context.RespondAsync(result).ConfigureAwait(false);
        }
    }
}