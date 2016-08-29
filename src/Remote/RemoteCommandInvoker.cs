using System;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Remote
{
    public sealed class RemoteMessageInvoker : IMessageInvoker
    {
        public RemoteProxy Proxy { get; }

        public RemoteMessageInvoker(RemoteChannel channel, string address)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            if (address == null)
                throw new ArgumentNullException(nameof(address));
            Proxy = channel.CreateProxy<RemoteProxy>(address);
        }

        public Task Invoke<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : class
        {
            throw new NotImplementedException();
        }

        public Task<TResult> Invoke<TResult, TQuery>(TQuery query, CancellationToken cancellationToken)
            where TQuery : class
            where TResult : class
        {
            var result = (TResult) Proxy.Invoke(query, typeof(TQuery), typeof(TResult));
            return Task.FromResult(result);
        }

        /// <summary>
        /// The .net remoting cannot work with generic methods and async\await.
        /// </summary>
        /// <seealso cref="System.MarshalByRefObject" />
        public sealed class RemoteProxy : MarshalByRefObject
        {
            private readonly IMessageInvoker messageInvoker;

            public RemoteProxy(IMessageInvoker messageInvoker)
            {
                this.messageInvoker = messageInvoker;
            }

            public object Invoke(object command, Type commandType, Type resultType)
            {
                var method = typeof(IMessageInvoker).GetMethod(nameof(messageInvoker.Invoke)).MakeGenericMethod(resultType, commandType);
                var respond = (Task) method.Invoke(messageInvoker, new[] { command, CancellationToken.None });
                respond.GetAwaiter().GetResult();
                var result = typeof(Task<>).MakeGenericType(resultType).GetProperty(@"Result").GetValue(respond);
                return result;
            }
        }
    }
}