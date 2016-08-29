using System.Reflection;
using System.Web;
using kolbasik.NCommandBus.Abstractions;
using kolbasik.NCommandBus.Core;
using kolbasik.NCommandBus.Host;
using Sample.Core;
using Sample.Handles;

namespace Sample.WebApp
{
    public class Global : HttpApplication
    {
        static Global()
        {
            var dependencyResolver = SampleDependencyResolver.Instance;
            dependencyResolver.Register(new Shop());
            dependencyResolver.RegisterTypes(typeof(IQueryHandler<,>), Assembly.Load("Sample.Handles"));
            dependencyResolver.RegisterTypes(typeof(ICommandHandler<>), Assembly.Load("Sample.Handles"));
            dependencyResolver.Register(new MessageBus(new InProcessMessageInvoker(dependencyResolver.ServiceContainer)));
        }
    }
}