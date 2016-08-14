using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Core;

namespace kolbasik.NCommandBus.Http
{
    public sealed class HttpCommandInvoker : ICommandInvoker, IDisposable
    {
        public HttpCommandInvoker(Uri requestUri)
            : this(requestUri, new MediaTypeFormatterCollection())
        {
        }

        public HttpCommandInvoker(Uri requestUri, MediaTypeFormatterCollection mediaTypeFormatterCollection)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));
            if (mediaTypeFormatterCollection == null)
                throw new ArgumentNullException(nameof(mediaTypeFormatterCollection));
            RequestUri = requestUri;
            MediaTypeFormatterCollection = mediaTypeFormatterCollection;
            MediaTypeFormatter = MediaTypeFormatterCollection.JsonFormatter;
            HttpClient = new HttpClient();
        }

        public Uri RequestUri { get; }
        public HttpClient HttpClient { get; }
        public MediaTypeFormatter MediaTypeFormatter { get; set; }
        public MediaTypeFormatterCollection MediaTypeFormatterCollection { get; }

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        public async Task<TResult> Invoke<TResult, TCommand>(TCommand command, CancellationToken cancellationToken)
        {
            var commandType = typeof(TCommand);
            var requestContent = new ObjectContent<TCommand>(command, MediaTypeFormatter);
            requestContent.Headers.Add(@"X-RPC-CommandType", commandType.FullName);

            var post = HttpClient.PostAsync(RequestUri.AbsoluteUri, requestContent, cancellationToken);
            var response = await post.ConfigureAwait(false);
            var responseContent = response.Content;

            response.EnsureSuccessStatusCode();

            var httpResultType = responseContent.Headers.GetValues(@"X-RPC-ResultType").FirstOrDefault();
            var resultType = Type.GetType(httpResultType, false, true) ?? typeof(TResult);
            var result = await responseContent.ReadAsAsync(resultType, MediaTypeFormatterCollection).ConfigureAwait(false);
            return (TResult)result;
        }
    }
}