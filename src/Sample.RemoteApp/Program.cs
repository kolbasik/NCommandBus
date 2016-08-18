using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using kolbasik.NCommandBus.Host;
using kolbasik.NCommandBus.Remote;
using Sample.Core;

namespace Sample.RemoteApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Program().Run().GetAwaiter().GetResult();
        }

        private async Task Run()
        {
            var dependencyResolver = new SampleDependencyResolver();
            dependencyResolver.RegisterAll(typeof(ICommandHandler<,>), Assembly.Load("Sample.Handles"));

            try
            {
                using (RemoteChannel.Tcp(8081)) // NOTE: or RemoteChannel.Http(8080)
                {
                    var remoteCommandInvokerProxy = new RemoteCommandInvoker.RemoteProxy(new InProcessCommandInvoker(dependencyResolver.ServiceContainer));
                    ObjRef hostCommandBusRef = RemotingServices.Marshal(remoteCommandInvokerProxy, "RPC");
                    Console.WriteLine("RPC.URI: " + hostCommandBusRef.URI);

                    Console.WriteLine(@"Press any key to disconnect...");
                    Console.ReadKey(true);

                    RemotingServices.Disconnect(remoteCommandInvokerProxy);
                }
            }
            catch (Exception ex)
            {
                do
                {
                    Console.WriteLine(ex.ToString());
                } while ((ex = ex.InnerException) != null);
            }

            Console.WriteLine(@"Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
