#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Http.Handlers;
using UnityEngine;

namespace Http
{
    /// <summary>
    /// Represents an HTTP request being built, with configurable URL, method, headers,
    /// body, and response transformation. Allows sending requests and receiving typed responses.
    /// </summary>
    public class Request
    {
        public string url { get; }
        public Func<object?, string?>? transformer { get; private set; }
        public IHttpErrorHandler? errorHandler { get; private set; }
        public object? body { get; private set; }
        public int timeout { get; private set; }
        public CancellationToken? cancellationToken { get; private set; }
        public HttpMethod method { get; private set; }
        public ProgressCallback? progressCallback { get; private set; }
        public Dictionary<string, string> headers { get; } = new Dictionary<string, string>();
        
        private readonly Dictionary<string, string>? _engineHeaders;

        /// <summary>
        /// Initializes a new <see cref="Request"/> with the specified URL and optional engine headers.
        /// </summary>
        /// <param name="url">The target URL for this request.</param>
        /// <param name="engineHeaders">Optional default headers from an <see cref="HttpEngine"/> instance.</param>
        public Request(string url, Dictionary<string, string>? engineHeaders = null)
        {
            this.url = url;
            _engineHeaders = engineHeaders;
        }
        
        /// <summary>
        /// Gets all headers for this request, combining engine default headers with request-specific headers.
        /// Request-specific headers take precedence over engine defaults.
        /// </summary>
        /// <returns>A dictionary containing all headers to be sent with this request.</returns>
        internal Dictionary<string, string> GetAllHeaders()
        {
            var allHeaders = new Dictionary<string, string>();

            if (_engineHeaders != null)
            {
                foreach (var header in _engineHeaders)
                {
                    allHeaders[header.Key] = header.Value;
                }
            }

            foreach (var header in headers)
            {
                allHeaders[header.Key] = header.Value;
            }

            return allHeaders;
        }

        /// <summary>
        /// Sets the HTTP method for this request (GET, POST, etc.).
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <returns>This <see cref="Request"/> instance for method chaining.</returns>
        public Request SetMethod(HttpMethod method)
        {
            this.method = method;
            return this;
        }

        /// <summary>
        /// Sets a custom error handler for this request to handle HTTP errors in a specific way.
        /// </summary>
        /// <param name="errorHandler">The error handler to use for this request.</param>
        /// <returns>This <see cref="Request"/> instance for method chaining.</returns>
        public Request SetErrorHandler(IHttpErrorHandler errorHandler)
        {
            this.errorHandler = errorHandler;
            return this;
        }

        /// <summary>
        /// Adds a header to this request. This header will be sent along with the request.
        /// </summary>
        /// <param name="key">The header name.</param>
        /// <param name="value">The header value.</param>
        /// <returns>This <see cref="Request"/> instance for method chaining.</returns>
        public Request SetHeader(string key, string value)
        {
            headers.Add(key, value);
            return this;
        }

        /// <summary>
        /// Sets the timeout for this request in seconds.
        /// </summary>
        /// <param name="timeout">The timeout in seconds.</param>
        /// <returns>This <see cref="Request"/> instance for method chaining.</returns>
        public Request SetTimeout(int timeout)
        {
            this.timeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets a cancellation token that can be used to cancel this request.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>This <see cref="Request"/> instance for method chaining.</returns>
        public Request SetCancellationToken(CancellationToken? cancellationToken)
        {
            if (cancellationToken == null)
            {
                return this;
            }
            
            this.cancellationToken = cancellationToken;
            return this;
        }

        /// <summary>
        /// Sets a custom transformer function that will be used to serialize the request body.
        /// </summary>
        /// <param name="transformer">A function that takes an object and returns its string representation.</param>
        /// <returns>This <see cref="Request"/> instance for method chaining.</returns>
        public Request SetTransformer(Func<object?, string?> transformer)
        {
            this.transformer = transformer;
            return this;
        }

        /// <summary>
        /// Sets the body content for this request. The body will be serialized using the configured transformer.
        /// </summary>
        /// <param name="body">The object to be sent as the request body.</param>
        /// <returns>This <see cref="Request"/> instance for method chaining.</returns>
        public Request SetBody(object body)
        {
            this.body = body;
            return this;
        }

        /// <summary>
        /// Sets a progress callback that will be invoked during the request to report download progress.
        /// This works with all response types (string, byte, etc.).
        /// </summary>
        /// <param name="progressCallback">The callback function to receive progress updates.</param>
        /// <returns>This <see cref="Request"/> instance for method chaining.</returns>
        public Request SetProgressCallback(ProgressCallback progressCallback)
        {
            this.progressCallback = progressCallback;
            return this;
        }

        /// <summary>
        /// Configures this request to return a <see cref="ByteResponse"/> containing binary data.
        /// </summary>
        /// <returns>An output handler that will return binary response data.</returns>
        public IOutputHandler<ByteResponse> SetByteOutput()
        {
            return new OutputByteHandler(this);
        }

        /// <summary>
        /// Configures this request to return a <see cref="StringResponse"/> containing text data.
        /// </summary>
        /// <returns>An output handler that will return string response data.</returns>
        public IOutputHandler<StringResponse> SetStringOutput()
        {
            return new StringObjectHandler(this);
        }

        /// <summary>
        /// Sends this request and returns the response as a <see cref="StringResponse"/>.
        /// This is equivalent to calling <c>SetStringOutput().Send()</c>.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the string response.</returns>
        public async Awaitable<StringResponse> Send()
        {
            return await SetStringOutput()
                .Send();
        }
    }
}