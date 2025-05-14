#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace Http.Handlers
{
    /// <summary>
    /// Handles sending an HTTP request and returning the response as a string.
    /// </summary>
    public class StringObjectHandler : IOutputHandler<StringResponse>
    {
        private readonly Request _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringObjectHandler"/> class
        /// for the given <see cref="Request"/>.
        /// </summary>
        /// <param name="request">The configured HTTP request to send.</param>
        public StringObjectHandler(Request request)
        {
            _request = request;
        }

        /// <summary>
        /// Sends the HTTP request asynchronously and parses the response body as a string.
        /// </summary>
        /// <returns>
        /// A task that completes with a <see cref="StringResponse"/> containing the response text.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown if the underlying request fails due to connection, protocol, or data processing errors.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the HTTP method on the request is not supported by this handler.
        /// </exception>
        public async Awaitable<StringResponse> Send()
        {
            string response = _request.method switch
            {
                HttpMethod.Get => await RequestHandler.CreateStringRequest(
                    _request.method,
                    _request.url,
                    _request.timeout,
                    _request.headers,
                    cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

                HttpMethod.Post => await RequestHandler.CreatePayloadRequest(
                    HttpMethod.Post,
                    _request.url,
                    _request.timeout,
                    _request.transformer?.Invoke(_request.body),
                    _request.headers,
                    _request.errorHandler,
                    cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

                _ => throw new InvalidOperationException("Invalid HTTP method.")
            };

            return new StringResponse(response);
        }
    }
}