using System;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using kolbasik.NCommandBus.Core;
using kolbasik.NCommandBus.Web;
using Sample.Core;

namespace Sample.WebApp
{
    public sealed class MessageBusHttpHandler : HttpTaskAsyncHandler
    {
        private readonly MessageBusHttpRpc messageBusHttpRpc;

        public MessageBusHttpHandler() : this(SampleDependencyResolver.Instance.Resolve<MessageBus>(), GlobalConfiguration.Configuration.Formatters)
        {
        }

        public MessageBusHttpHandler(MessageBus messageBus, MediaTypeFormatterCollection mediaTypeFormatterCollection)
        {
            if (messageBus == null)
                throw new ArgumentNullException(nameof(messageBus));

            messageBusHttpRpc = new MessageBusHttpRpc(messageBus, mediaTypeFormatterCollection);
        }

        public override bool IsReusable => true;

        public override Task ProcessRequestAsync(HttpContext context)
        {
            return messageBusHttpRpc.ProcessRequest(new HttpContextWrapper(context));
        }
    }
}