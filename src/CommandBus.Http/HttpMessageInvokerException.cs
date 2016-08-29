using System;

namespace kolbasik.NCommandBus.Http
{
    public sealed class HttpMessageInvokerException : Exception
    {
        public HttpMessageInvokerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public string RequestContent { get; set; }
        public string ResponseContent { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, RequestContent: {RequestContent}, ResponseContent: {ResponseContent}";
        }
    }
}