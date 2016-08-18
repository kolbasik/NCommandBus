using System;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Remote
{
    public sealed class RemoteCommandInvoker : ICommandInvoker
    {
        public RemoteProxy Proxy { get; }

        public RemoteCommandInvoker(RemoteChannel channel, string address)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            if (address == null)
                throw new ArgumentNullException(nameof(address));
            Proxy = channel.CreateProxy<RemoteProxy>(address);
        }

        public Task<TResult> Invoke<TResult, TCommand>(TCommand command, CancellationToken cancellationToken)
            where TCommand : class
            where TResult : class
        {
            var result = (TResult) Proxy.Invoke(command, typeof(TCommand), typeof(TResult));
            return Task.FromResult(result);
        }

        /// <summary>
        /// The .net remoting cannot work with generic methods and async\await.
        /// </summary>
        /// <seealso cref="System.MarshalByRefObject" />
        public sealed class RemoteProxy : MarshalByRefObject
        {
            private readonly ICommandInvoker commandInvoker;

            public RemoteProxy(ICommandInvoker commandInvoker)
            {
                this.commandInvoker = commandInvoker;
            }

            public object Invoke(object command, Type commandType, Type resultType)
            {
                var method = typeof(ICommandInvoker).GetMethod(nameof(commandInvoker.Invoke)).MakeGenericMethod(resultType, commandType);
                var respond = (Task) method.Invoke(commandInvoker, new[] { command, CancellationToken.None });
                respond.GetAwaiter().GetResult();
                var result = typeof(Task<>).MakeGenericType(resultType).GetProperty(@"Result").GetValue(respond);
                return result;
            }
        }
    }
}