namespace Akakani.AspNetCore.DevMiddleware.Proxy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Primitives;


    internal class ConditionalProxyMiddleware
    {
        private readonly ConditionalProxyMiddlewareOptions options;
        private readonly string pathPrefix;
        private readonly bool pathPrefixIsRoot;
        private readonly RequestDelegate next;


        public ConditionalProxyMiddleware(RequestDelegate next, string pathPrefix, ConditionalProxyMiddlewareOptions options)
        {
            this.next = next;
            this.options = options;

            (this.pathPrefix, this.pathPrefixIsRoot) = NormalizePathPrefix(pathPrefix);
        }

        public async Task Invoke(HttpContext context)
        {
            // Attempt to proxy the request to the path we were configured to listen to
            if (context.Request.Path.StartsWithSegments(pathPrefix) || pathPrefixIsRoot)
            {
                if (await PerformProxyRequest(context))
                {
                    return;
                }
            }

            // Not a request we can proxy
            await next.Invoke(context);
        }

        private async Task<bool> PerformProxyRequest(HttpContext context)
        {
            // Is the client requesting streaming ?
            bool isStreamingRequest = IsStreamingRequest(context);

            // Calculate destination Uri - $"{options.Scheme}://{options.Host}:{options.Port}{context.Request.Path}{context.Request.QueryString}"
            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = options.Scheme,
                Host = options.Host,
                Port = options.Port,
                Path = context.Request.Path,
                Query = context.Request.QueryString.Value
            };

            // Prepare a request to the destination Uri using the same method as the request we are handling
            HttpMethod httpMethod = new HttpMethod(context.Request.Method);
            using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(httpMethod, uriBuilder.Uri))
            {
                // Specify destination 'Host' header: $"{options.Host}:{options.Port}"
                httpRequestMessage.Headers.Host = options.Host + ":" + options.Port;

                // Copy request headers to the destination request headers
                foreach (KeyValuePair<string, StringValues> header in context.Request.Headers)
                {
                    // Add pertinent headers to the request - Some headers ...
                    if (!httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.AsEnumerable<string>()))
                    {
                        // ... need to be set on the request content instead
                        httpRequestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.AsEnumerable<string>());
                    }
                }

                // Get an HttpClient adequate for the request - We do not use HttpClientFactory here because this is for development only so we are not worried about performances as much
                using (HttpClient httpClient = isStreamingRequest ? GetStreamingHttpClient() : GetHttpClient())
                {
                    // Send the request to the destination - We consider the request "complete" when headers have been read
                    using (HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                    {
                        // Continue proxying if the destination returns anything but "404 - Not Found"
                        if (httpResponseMessage.StatusCode != HttpStatusCode.NotFound)
                        {
                            // Server-Sent-Events streams (like Webpack HMR) are long lived connections over which data is occasionally exchanged
                            // See https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events for more details
                            // For streaming protocols we disable all response buffering - Any byte written to the response should imediately be sent to the client
                            if (isStreamingRequest)
                            {
                                DisableResponseBuffering(context);
                            }

                            // Pass status code back
                            context.Response.StatusCode = (int)httpResponseMessage.StatusCode;

                            // Pass response headers back
                            foreach (KeyValuePair<string, IEnumerable<string>> header in httpResponseMessage.Headers)
                            {
                                context.Response.Headers[header.Key] = header.Value.ToArray();
                            }

                            // Pass response content headers back
                            foreach (KeyValuePair<string, IEnumerable<string>> contentHeader in httpResponseMessage.Content.Headers)
                            {
                                context.Response.Headers[contentHeader.Key] = contentHeader.Value.ToArray();
                            }

                            // Remove "Transfer-Encoding" if provided - CopyToAsync() / FlushAsync() below performs the correct negotiation
                            context.Response.Headers.Remove("Transfer-Encoding");

                            // Read the stream from the destination and copy it back to our request - The stream will close when the destination closes the connection
                            using (Stream responseStream = await httpResponseMessage.Content.ReadAsStreamAsync())
                            {
                                try
                                {
                                    if (isStreamingRequest)
                                    {
                                        // Connect the response stream from the destination to the response stream of the connection we are handling
                                        using (StreamContent streamContent = new StreamContent(responseStream))
                                        {
                                            await streamContent.CopyToAsync(context.Response.Body);
                                        }
                                    }
                                    else
                                    {
                                        await responseStream.CopyToAsync(context.Response.Body, options.HttpBufferSize, context.RequestAborted);
                                        await context.Response.Body.FlushAsync();
                                    }
                                }
                                catch (SocketException)
                                {
                                    // The CopyToAsync task will throw SocketException ("An existing connection was forcibly closed by the remote host") when the ASP.NET Core application shuts down via Ctrl-C. Don't treat this as an error
                                }
                                catch (HttpRequestException)
                                {
                                    // The CopyToAsync task will throw HttpRequestxception ("Error while copying content to a stream") when the ASP.NET Core application shuts down via Ctrl-C. Don't treat this as an error
                                }
                                catch (OperationCanceledException)
                                {
                                    // The CopyToAsync task will be canceled if the client disconnects (e.g., user closes or refreshes the browser tab). Don't treat this as an error
                                }
                            }

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }

        private static bool IsStreamingRequest(HttpContext context)
        {
            bool isStreaming = false;
            if (context.Request.Headers.TryGetValue("Accept", out StringValues acceptStringValues))
            {
                isStreaming = acceptStringValues.Contains("text/event-stream", StringComparer.OrdinalIgnoreCase);
            }

            return isStreaming;
        }

        private static (string normalizedPathPrefix, bool isPathPrefixRoot) NormalizePathPrefix(string pathPrefix)
        {
            string normalizedPathPrefix = !pathPrefix.StartsWith("/") ? "/" + pathPrefix : pathPrefix;
            bool pathIsRoot = string.Equals(normalizedPathPrefix, "/", StringComparison.Ordinal);

            return (normalizedPathPrefix, pathIsRoot);
        }

        private HttpClient GetHttpClient()
        {
            // Get an HttpClient specifically configure to connect to the proxied address
            HttpClient proxyHttpClient = new HttpClient(new HttpClientHandler());

            // Configure timeout as request by user
            proxyHttpClient.Timeout = options.RequestTimeout;

            // For simplicity, access proxied servers with HTTP 1.1 - Configure our client accordingly
            proxyHttpClient.DefaultRequestVersion = new Version(1, 1);
#if NET5_0
            proxyHttpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
#endif
            return proxyHttpClient;
        }

        private HttpClient GetStreamingHttpClient()
        {
            // Get an HttpClient specifically configure to connect to the proxied address
            HttpClient streamingHttpClient = GetHttpClient();

            // Server-Sent-Events streams (like Webpack HMR) are long lived connections over which data is occasionally exchanged
            // See https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events for more details
            // For streaming protocols we ...

            // ... Configure an infinite timeout to ensure the connection won't close on us prematurely
            streamingHttpClient.Timeout = Timeout.InfiniteTimeSpan;

            // ... Disable all buffering so the response stream will alert us when there is at least one byte to read
            streamingHttpClient.MaxResponseContentBufferSize = 1;

            return streamingHttpClient;
        }

        private static void DisableResponseBuffering(HttpContext context)
        {
            IHttpResponseBodyFeature responseBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();
            if (responseBodyFeature != null)
            {
                // Has to be done before the first WriteAsync()
                responseBodyFeature.DisableBuffering();
            }
        }
    }
}
