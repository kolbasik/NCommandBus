using System.Reflection;
using System.Web;
using kolbasik.NCommandBus.Abstractions;
using kolbasik.NCommandBus.Core;
using kolbasik.NCommandBus.Host;
using Sample.Core;

namespace Sample.WebApp
{
    public class Global : HttpApplication
    {
        static Global()
        {
            var dependencyResolver = SampleDependencyResolver.Instance;
            dependencyResolver.RegisterAll(typeof(ICommandHandler<,>), Assembly.Load("Sample.Handles"));
            dependencyResolver.Register(typeof(CommandBus), new CommandBus(new HostCommandInvoker(dependencyResolver.ServiceContainer)));
        }
    }
}