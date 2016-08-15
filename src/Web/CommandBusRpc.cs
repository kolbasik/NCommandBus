using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using kolbasik.NCommandBus.Core;
using Newtonsoft.Json;

namespace kolbasik.NCommandBus.Web
{
    public sealed class CommandBusRpc
    {
        private readonly CommandBus commandBus;

        public CommandBusRpc(CommandBus commandBus)
        {
            if (commandBus == null) throw new ArgumentNullException(nameof(commandBus));
            this.commandBus = commandBus;
        }

        public async Task ProcessRequest(HttpContextBase context)
        {
            // NOTE: var result = await commandBus.Send<TResult, TCommand>(command);
            var httpRequest = context.Request;
            var httpResponse = context.Response;
            httpResponse.ContentEncoding = Encoding.UTF8;
            try
            {
                var httpCommandType = httpRequest.Params[@"commandType"] ?? httpRequest.Headers.GetValues(@"X-RPC-CommandType").FirstOrDefault();
                var httpResultType = httpRequest.Params[@"resultType"] ?? httpRequest.Headers.GetValues(@"X-RPC-ResultType").FirstOrDefault();

                var commandType = Type.GetType(httpCommandType, false, true);
                var resultType = Type.GetType(httpResultType, false, true);
                if (commandType == null || resultType == null)
                {
                    throw new ArgumentException(@"Could not resolve the command or result types.");
                }

                var input = await new StreamReader(httpRequest.InputStream, Encoding.UTF8).ReadToEndAsync().ConfigureAwait(false);
                var command = JsonConvert.DeserializeObject(input, commandType);
                if (command == null)
                {
                    throw new ArgumentException(@"The command body is required.");
                }

                var method = typeof(CommandBus).GetMethod(nameof(commandBus.Send)).MakeGenericMethod(resultType, commandType);
                var respond = (Task)method.Invoke(commandBus, new[] { command, httpResponse.ClientDisconnectedToken });
                await respond.ConfigureAwait(false);

                var result = typeof(Task<>).MakeGenericType(resultType).GetProperty(@"Result").GetValue(respond);
                var output = JsonConvert.SerializeObject(result);

                httpResponse.StatusCode = (int)HttpStatusCode.OK;
                httpResponse.Headers.Add(@"X-RPC-ResultType", httpResultType);
                httpResponse.ContentType = @"application/json";
                httpResponse.Write(output);
            }
            catch (ArgumentException ex)
            {
                httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                httpResponse.StatusDescription = ex.Message.Replace(Environment.NewLine, @" ");
                httpResponse.ContentType = @"application/json";
                httpResponse.Write(JsonConvert.SerializeObject(ex));
            }
        }
    }
}
