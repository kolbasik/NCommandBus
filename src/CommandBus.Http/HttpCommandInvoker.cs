using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Http
{
    public sealed class HttpCommandInvoker : ICommandInvoker, IDisposable
    {
        public HttpCommandInvoker(Uri requestUri)
            : this(requestUri, new HttpClientHandler(), new MediaTypeFormatterCollection())
        {
        }

        public HttpCommandInvoker(Uri requestUri, HttpMessageHandler httpMessageHandler, MediaTypeFormatterCollection mediaTypeFormatterCollection)
        {
            if (requestUri == null) throw new ArgumentNullException(nameof(requestUri));
            if (httpMessageHandler == null) throw new ArgumentNullException(nameof(httpMessageHandler));
            if (mediaTypeFormatterCollection == null) throw new ArgumentNullException(nameof(mediaTypeFormatterCollection));
            RequestUri = requestUri;
            MediaTypeFormatterCollection = mediaTypeFormatterCollection;
            MediaTypeFormatter = MediaTypeFormatterCollection.JsonFormatter;
            HttpClient = new HttpClient(httpMessageHandler);
        }

        public Uri RequestUri { get; }
        public HttpClient HttpClient { get; }
        public MediaTypeFormatter MediaTypeFormatter { get; set; }
        public MediaTypeFormatterCollection MediaTypeFormatterCollection { get; }

        public async Task<TResult> Invoke<TResult, TCommand>(TCommand command, CancellationToken cancellationToken)
        {
            var requestContent = new ObjectContent<TCommand>(command, MediaTypeFormatter);
            requestContent.Headers.Add(@"X-RPC-CommandType", GetTypeName(typeof (TCommand)));
            requestContent.Headers.Add(@"X-RPC-ResultType", GetTypeName(typeof(TResult)));

            var post = HttpClient.PostAsync(RequestUri.AbsoluteUri, requestContent, cancellationToken);
            var response = await post.ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseContent = response.Content;
            if (responseContent != null)
            {
                IEnumerable<string> values;
                var resultType = responseContent.Headers.TryGetValues(@"X-RPC-ResultType", out values) ? values.Select(GetType).FirstOrDefault() : typeof(TResult);
                var result = await responseContent.ReadAsAsync(resultType, MediaTypeFormatterCollection).ConfigureAwait(false);
                return (TResult)result;
            }
            return default(TResult);
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        private static Type GetType(string typeName)
        {
            return Type.GetType(typeName, false, true);
        }

        private static string GetTypeName(Type type)
        {
            return $@"{type.FullName}, {type.Assembly.FullName}";
        }
    }
}