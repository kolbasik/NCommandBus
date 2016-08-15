using System;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using kolbasik.NCommandBus.Core;

namespace kolbasik.NCommandBus.Web
{
    public sealed class CommandBusRpc
    {
        private readonly CommandBus commandBus;
        private MediaTypeFormatterCollection MediaTypeFormatters;

        public CommandBusRpc(CommandBus commandBus, MediaTypeFormatterCollection mediaTypeFormatterCollection)
        {
            if (commandBus == null) throw new ArgumentNullException(nameof(commandBus));
            if (mediaTypeFormatterCollection == null) throw new ArgumentNullException(nameof(mediaTypeFormatterCollection));
            this.commandBus = commandBus;
            MediaTypeFormatters = mediaTypeFormatterCollection;
        }

        public async Task ProcessRequest(HttpContextBase context)
        {
            // NOTE: var result = await commandBus.Send<TResult, TCommand>(command);
            var httpRequest = context.Request;
            var httpResponse = context.Response;
            httpResponse.ContentEncoding = Encoding.UTF8;
            try
            {
                var httpCommandType = httpRequest.Params[@"commandType"] ?? httpRequest.Headers.GetValues(@"X-RPC-CommandType")?.FirstOrDefault();
                var httpResultType = httpRequest.Params[@"resultType"] ?? httpRequest.Headers.GetValues(@"X-RPC-ResultType")?.FirstOrDefault();
                var commandType = Type.GetType(httpCommandType, false, true);
                var resultType = Type.GetType(httpResultType, false, true);
                if (commandType == null || resultType == null)
                {
                    throw new ArgumentException(@"Could not resolve the command or result types.");
                }

                var mediaTypeFormatter = MediaTypeFormatters.FindReader(commandType, new MediaTypeHeaderValue(httpRequest.ContentType.Split(';')[0]));
                var command = await mediaTypeFormatter.ReadFromStreamAsync(commandType, httpRequest.InputStream, null, null).ConfigureAwait(false);
                if (command == null)
                {
                    throw new ArgumentException(@"The command body is required.");
                }

                var method = typeof(CommandBus).GetMethod(nameof(commandBus.Send)).MakeGenericMethod(resultType, commandType);
                var respond = (Task)method.Invoke(commandBus, new object[] { command, httpResponse.ClientDisconnectedToken });
                await respond.ConfigureAwait(false);

                var result = typeof(Task<>).MakeGenericType(resultType).GetProperty(@"Result").GetValue(respond);

                httpResponse.StatusCode = (int)HttpStatusCode.OK;
                httpResponse.Headers.Add(@"X-RPC-ResultType", httpResultType);
                httpResponse.ContentType = mediaTypeFormatter.SupportedMediaTypes.First().MediaType;
                await mediaTypeFormatter.WriteToStreamAsync(resultType, result, httpResponse.OutputStream, null, null).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                httpResponse.StatusDescription = ex.Message.Replace(Environment.NewLine, @" ");
                httpResponse.ContentType = @"application/json";
                await MediaTypeFormatters.JsonFormatter.WriteToStreamAsync(typeof(Exception), ex, httpResponse.OutputStream, null, null).ConfigureAwait(false);
            }
        }
    }
}
