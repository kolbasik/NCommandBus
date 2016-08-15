using System.Reflection;
using System.Web;
using kolbasik.NCommandBus.Abstractions;
using kolbasik.NCommandBus.Core;
using kolbasik.NCommandBus.Host;

namespace Sample.WebApp
{
    public class Global : HttpApplication
    {
        static Global()
        {
            var serviceLocator = ServiceLocator.Instance;
            serviceLocator.RegisterAll(typeof(ICommandHandler<,>), Assembly.Load("Sample.Handles"));
            serviceLocator.Register(typeof(CommandBus), new CommandBus(new HostCommandInvoker(serviceLocator.ServiceContainer)));
        }
    }
}