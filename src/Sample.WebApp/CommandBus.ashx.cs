using System;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using kolbasik.NCommandBus.Core;
using kolbasik.NCommandBus.Web;

namespace Sample.WebApp
{
    public sealed class CommandBusHttpHandler : HttpTaskAsyncHandler
    {
        private readonly CommandBusRpc commandBusRpc;

        public CommandBusHttpHandler() : this(ServiceLocator.Instance.Resolve<CommandBus>(), GlobalConfiguration.Configuration.Formatters)
        {
        }

        public CommandBusHttpHandler(CommandBus commandBus, MediaTypeFormatterCollection mediaTypeFormatterCollection)
        {
            if (commandBus == null)
                throw new ArgumentNullException(nameof(commandBus));

            commandBusRpc = new CommandBusRpc(commandBus, mediaTypeFormatterCollection);
        }

        public override bool IsReusable => true;

        public override Task ProcessRequestAsync(HttpContext context)
        {
            return commandBusRpc.ProcessRequest(new HttpContextWrapper(context));
        }
    }
}