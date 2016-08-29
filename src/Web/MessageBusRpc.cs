using System;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Web
{
    public sealed class MessageBusHttpRpc
    {
        private readonly IMessageBus messageBus;
        private readonly MediaTypeFormatterCollection mediaTypeFormatters;

        public MessageBusHttpRpc(IMessageBus messageBus, MediaTypeFormatterCollection mediaTypeFormatterCollection)
        {
            if (messageBus == null)
                throw new ArgumentNullException(nameof(messageBus));
            if (mediaTypeFormatterCollection == null)
                throw new ArgumentNullException(nameof(mediaTypeFormatterCollection));
            this.messageBus = messageBus;
            mediaTypeFormatters = mediaTypeFormatterCollection;
        }

        public async Task ProcessRequest(HttpContextBase context)
        {
            /* NOTE:
                await messageBus.Send<TCommand>(command);
                var result = await messageBus.Ask<TResult, TQuery>(query); */

            var httpRequest = context.Request;
            var httpResponse = context.Response;
            httpResponse.ContentEncoding = Encoding.UTF8;
            try
            {
                object result = null;
                var httpMessageType = httpRequest.Params[@"messageType"] ?? httpRequest.Headers.GetValues(@"X-RPC-MessageType")?.FirstOrDefault();
                var messageType = Type.GetType(httpMessageType, false, true);
                if (messageType == null)
                {
                    throw new ArgumentException(@"Could not resolve the message type.");
                }

                var mediaTypeFormatter = mediaTypeFormatters.FindReader(messageType, new MediaTypeHeaderValue(httpRequest.ContentType.Split(';')[0]));
                var message = await mediaTypeFormatter.ReadFromStreamAsync(messageType, httpRequest.InputStream, null, null).ConfigureAwait(false);
                if (message == null)
                {
                    throw new ArgumentException(@"The message body is required.");
                }

                var action = httpRequest.Params[@"action"] ?? httpRequest.Headers.GetValues(@"X-RPC-Action")?.FirstOrDefault() ?? string.Empty;
                switch (action.ToLowerInvariant())
                {
                    case "tell":
                    {
                        var method = typeof(ICommandBus).GetMethod(nameof(messageBus.Tell)).MakeGenericMethod(messageType);
                        var respond = (Task) method.Invoke(messageBus, new[] { message, httpResponse.ClientDisconnectedToken });
                        await respond.ConfigureAwait(false);

                        httpResponse.StatusCode = (int) HttpStatusCode.NoContent;
                        break;
                    }
                    case "ask":
                    {
                        var httpResultType = httpRequest.Params[@"resultType"] ?? httpRequest.Headers.GetValues(@"X-RPC-ResultType")?.FirstOrDefault();
                        var resultType = Type.GetType(httpResultType, false, true);
                        if (resultType == null)
                        {
                            throw new ArgumentException(@"Could not resolve the result type.");
                        }

                        var method = typeof(IQueryBus).GetMethod(nameof(messageBus.Ask)).MakeGenericMethod(resultType, messageType);
                        var respond = (Task) method.Invoke(messageBus, new[] { message, httpResponse.ClientDisconnectedToken });
                        await respond.ConfigureAwait(false);
                        result = ((dynamic) respond).Result;

                        httpResponse.StatusCode = (int)HttpStatusCode.OK;
                        httpResponse.Headers.Add(@"X-RPC-ResultType", httpResultType);
                        httpResponse.ContentType = mediaTypeFormatter.SupportedMediaTypes.First().MediaType;
                        await mediaTypeFormatter.WriteToStreamAsync(resultType, result, httpResponse.OutputStream, null, null).ConfigureAwait(false);
                        break;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                httpResponse.StatusCode = (int) HttpStatusCode.BadRequest;
                httpResponse.StatusDescription = ex.Message.Replace(Environment.NewLine, @" ");
                httpResponse.ContentType = @"application/json";
                await mediaTypeFormatters.JsonFormatter.WriteToStreamAsync(typeof(Exception), ex, httpResponse.OutputStream, null, null).ConfigureAwait(false);
            }
        }
    }
}