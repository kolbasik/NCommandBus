using System;
using System.Threading.Tasks;
using System.Web;
using kolbasik.NCommandBus.Core;
using kolbasik.NCommandBus.Web;
using Sample.WebApp.AspNet;

namespace Sample.WebApp
{
    public sealed class CommandBusHttpHandler : HttpTaskAsyncHandler
    {
        private readonly CommandBusRpc commandBusRpc;

        public CommandBusHttpHandler() : this(ServiceLocator.Instance.Resolve<CommandBus>())
        {
        }

        public CommandBusHttpHandler(CommandBus commandBus)
        {
            if (commandBus == null) throw new ArgumentNullException(nameof(commandBus));

            this.commandBusRpc = new CommandBusRpc(commandBus);
        }

        public override bool IsReusable => true;

        public override Task ProcessRequestAsync(HttpContext context)
        {
            return commandBusRpc.ProcessRequest(new HttpContextWrapper(context));
        }
    }
}