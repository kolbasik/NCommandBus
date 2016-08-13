using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Core;

namespace kolbasik.NCommandBus.Http
{
    public sealed class HttpCommandBus : CommandBus, IDisposable
    {
        public Uri RequestUri { get; }
        public HttpClient HttpClient { get; }
        public MediaTypeFormatter MediaTypeFormatter { get; set; }
        public MediaTypeFormatterCollection MediaTypeFormatterCollection { get; }

        public HttpCommandBus(Uri requestUri)
            : this(requestUri, new MediaTypeFormatterCollection())
        {
        }

        public HttpCommandBus(Uri requestUri, MediaTypeFormatterCollection mediaTypeFormatterCollection)
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

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        protected override async Task<TResult> Execute<TResult, TCommand>(TCommand command)
        {
            var post = HttpClient.PostAsync(RequestUri.AbsoluteUri, command, MediaTypeFormatter, CancellationToken.None);
            var response = await post.ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<TResult>(MediaTypeFormatterCollection).ConfigureAwait(false);
            return result;
        }
    }
}
