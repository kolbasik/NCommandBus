using System;
using System.ComponentModel.Design;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using kolbasik.NCommandBus.Host;
using kolbasik.NCommandBus.Remote;
using Sample.Commands;
using Sample.Handles;

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
            var serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(ICommandHandler<AddValues, AddValuesResult>), new Calculator());
            serviceContainer.AddService(typeof(ICommandHandler<SubValues, SubValuesResult>), new Calculator());
            serviceContainer.AddService(typeof(ICommandHandler<GetAppName, GetAppName.Result>), new AppDataHandler());

            try
            {
                using (RemoteChannel.Tcp(8081)) // NOTE: or RemoteChannel.Http(8080)
                {
                    var remoteCommandInvokerProxy = new RemoteCommandInvoker.RemoteProxy(new HostCommandInvoker(serviceContainer));
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
