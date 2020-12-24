namespace Akakani.AspNetCore.DevMiddleware.Proxy
{
    using System;


    internal class ConditionalProxyMiddlewareOptions
    {
        public string Scheme { get; }
        public string Host { get; }
        public int Port { get; }
        public int HttpBufferSize { get; }
        public TimeSpan RequestTimeout { get; }

        public ConditionalProxyMiddlewareOptions(string scheme, string host, int port, int httpBufferSize, TimeSpan requestTimeout)
        {
            Scheme = scheme;
            Host = host;
            Port = port;
            HttpBufferSize = httpBufferSize;
            RequestTimeout = requestTimeout;
        }
    }
}
