using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Core
{
    public sealed class MessageBus : IMessageBus
    {
        private readonly IMessageInvoker messageInvoker;

        public MessageBus(IMessageInvoker messageInvoker)
        {
            if (messageInvoker == null) throw new ArgumentNullException(nameof(messageInvoker));
            this.messageInvoker = messageInvoker;
            MessageObservers = new List<IMessageObserver> {ValidationInterceptor.Instance};
            MessageValidators = new List<IMessageValidator> {ValidationInterceptor.Instance};
        }

        public List<IMessageObserver> MessageObservers { get; }
        public List<IMessageValidator> MessageValidators { get; }

        public async Task Tell<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : class
        {
            var context = new MessageContext<TCommand, object>(command);

            foreach (var messageValidator in MessageValidators)
                await messageValidator.Validate(context).ConfigureAwait(false);

            foreach (var messageObserver in MessageObservers)
                await messageObserver.PreInvoke(context).ConfigureAwait(false);

            await messageInvoker.Invoke<TCommand>(context.Message, cancellationToken).ConfigureAwait(false);

            foreach (var messageObserver in MessageObservers)
                await messageObserver.PostInvoke(context).ConfigureAwait(false);
        }

        public async Task<TResult> Ask<TResult, TQuery>(TQuery query, CancellationToken cancellationToken) where TResult : class where TQuery : class
        {
            var context = new MessageContext<TQuery, TResult>(query);

            foreach (var messageValidator in MessageValidators)
                await messageValidator.Validate(context).ConfigureAwait(false);

            foreach (var messageObserver in MessageObservers)
                await messageObserver.PreInvoke(context).ConfigureAwait(false);

            context.Result = await messageInvoker.Invoke<TResult, TQuery>(context.Message, cancellationToken).ConfigureAwait(false);

            foreach (var messageObserver in MessageObservers)
                await messageObserver.PostInvoke(context).ConfigureAwait(false);

            return context.Result;
        }
    }
}