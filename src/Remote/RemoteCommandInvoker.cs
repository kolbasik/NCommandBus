using System;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Remote
{
    public sealed class RemoteCommandInvoker : ICommandInvoker
    {
        public RemoteProxy Proxy { get; }

        public RemoteCommandInvoker(RemoteProxy proxy)
        {
            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy));
            Proxy = proxy;
        }

        public Task<TResult> Invoke<TResult, TCommand>(TCommand command, CancellationToken cancellationToken)
        {
            var result = (TResult) Proxy.Invoke(command, typeof(TCommand), typeof(TResult));
            return Task.FromResult(result);
        }

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