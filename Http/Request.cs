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
        public CancellationTokenSource? cancellationToken { get; private set; }
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

        public Request SetHeader(string key, string value)
        {
            headers.Add(key, value);
            return this;
        }

        public Request SetTimeout(int timeout)
        {
            this.timeout = timeout;
            return this;
        }

        public Request SetCancellationToken(CancellationTokenSource? cancellationToken)
        {
            if (cancellationToken == null)
            {
                return this;
            }
            
            this.cancellationToken = cancellationToken;
            return this;
        }

        public Request SetTransformer(Func<object?, string?> transformer)
        {
            this.transformer = transformer;
            return this;
        }

        public Request SetBody(object body)
        {
            this.body = body;
            return this;
        }

        public Request SetProgressCallback(ProgressCallback progressCallback)
        {
            this.progressCallback = progressCallback;
            return this;
        }

        public IOutputHandler<ByteResponse> SetByteOutput()
        {
            return new OutputByteHandler(this);
        }

        public IOutputHandler<StringResponse> SetStringOutput()
        {
            return new StringObjectHandler(this);
        }

        public async Awaitable<StringResponse> Send()
        {
            return await SetStringOutput()
                .Send();
        }
    }
}