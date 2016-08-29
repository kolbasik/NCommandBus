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
            dependencyResolver.RegisterTypes(typeof(IQueryHandler<,>), Assembly.Load("Sample.Handles"));
            dependencyResolver.Register(typeof(MessageBus), new MessageBus(new InProcessMessageInvoker(dependencyResolver.ServiceContainer)));
        }
    }
}