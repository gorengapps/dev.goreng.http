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
        /// <summary>
        /// The URL to which this request will be sent.
        /// </summary>
        public string url { get; }

        /// <summary>
        /// Optional function to transform a response object into a string payload for the request.
        /// </summary>
        public Func<object?, string?>? transformer { get; private set; }
        
        public IHttpErrorHandler? errorHandler { get; private set; }

        /// <summary>
        /// The request body object, which will be transformed using <see cref="transformer"/>.
        /// </summary>
        public object? body { get; private set; }
        
        public int timeout { get; private set; }
        
        public CancellationTokenSource? cancellationToken { get; private set; }

        /// <summary>
        /// The HTTP method to use (GET, POST, etc.).
        /// </summary>
        public HttpMethod method { get; private set; }

        /// <summary>
        /// Custom headers to include in the request.
        /// </summary>
        public Dictionary<string, string> headers { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new <see cref="Request"/> with the specified URL.
        /// </summary>
        /// <param name="url">The target URL for this request.</param>
        public Request(string url)
        {
            this.url = url;
        }

        /// <summary>
        /// Sets the HTTP method for this request.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <returns>The current <see cref="Request"/> instance for chaining.</returns>
        public Request SetMethod(HttpMethod method)
        {
            this.method = method;
            return this;
        }

        public Request SetErrorHandler(IHttpErrorHandler errorHandler)
        {
            this.errorHandler = errorHandler;
            return this;
        }

        /// <summary>
        /// Adds a header to the request.
        /// </summary>
        /// <param name="key">The header name.</param>
        /// <param name="value">The header value.</param>
        /// <returns>The current <see cref="Request"/> instance for chaining.</returns>
        public Request SetHeader(string key, string value)
        {
            headers.Add(key, value);
            return this;
        }

        /// <summary>
        /// Applies a timeout to the request
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Request SetTimeout(int timeout)
        {
            this.timeout = timeout;
            return this;
        }

        public Request SetCancellationToken(CancellationTokenSource cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            return this;
        }

        /// <summary>
        /// Sets a transformer function to convert the request body object into a string payload.
        /// </summary>
        /// <param name="transformer">A function that takes the body object and returns its serialized form.</param>
        /// <returns>The current <see cref="Request"/> instance for chaining.</returns>
        public Request SetTransformer(Func<object?, string?> transformer)
        {
            this.transformer = transformer;
            return this;
        }

        /// <summary>
        /// Assigns the body object for this request.
        /// </summary>
        /// <param name="body">The payload object to send.</param>
        /// <returns>The current <see cref="Request"/> instance for chaining.</returns>
        public Request SetBody(object body)
        {
            this.body = body;
            return this;
        }

        /// <summary>
        /// Configures the request to expect a byte array in the response.
        /// </summary>
        /// <returns>An <see cref="IOutputHandler{ByteResponse}"/> to send the request and parse the byte response.</returns>
        public IOutputHandler<ByteResponse> SetByteOutput()
        {
            return new OutputByteHandler(this);
        }

        /// <summary>
        /// Configures the request to expect a string response.
        /// </summary>
        /// <returns>An <see cref="IOutputHandler{StringResponse}"/> to send the request and parse the string response.</returns>
        public IOutputHandler<StringResponse> SetStringOutput()
        {
            return new StringObjectHandler(this);
        }

        /// <summary>
        /// Sends the request asynchronously, expecting a string response.
        /// </summary>
        /// <returns>A task that completes with the <see cref="StringResponse"/>.</returns>
        public async Awaitable<StringResponse> Send()
        {
            return await SetStringOutput()
                .Send();
        }
    }
}
