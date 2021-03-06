﻿using System;
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
            where TCommand : class
            where TResult : class
        {
            var context = new CommandContext<TCommand, TResult>(command);

            foreach (var commandValidator in CommandValidators)
                await commandValidator.Validate(context).ConfigureAwait(false);

            foreach (var commandObserver in CommandObservers)
                await commandObserver.PreInvoke(context).ConfigureAwait(false);

            context.Result = await commandInvoker.Invoke<TResult, TCommand>(context.Command, cancellationToken).ConfigureAwait(false);

            foreach (var commandObserver in CommandObservers)
                await commandObserver.PostInvoke(context).ConfigureAwait(false);

            return context.Result;
        }
    }
}