﻿#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace Http.Handlers
{
    /// <summary>
    /// Handles sending an HTTP request and returning the response as a byte array.
    /// </summary>
    public class OutputByteHandler : IOutputHandler<ByteResponse>
    {
        private readonly Request _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputByteHandler"/> class
        /// for the given <see cref="Request"/>.
        /// </summary>
        /// <param name="request">The configured HTTP request to send.</param>
        public OutputByteHandler(Request request)
        {
            _request = request;
        }

        /// <summary>
        /// Sends the HTTP request asynchronously and parses the response body as a byte array.
        /// </summary>
        /// <returns>
        /// A task that completes with a <see cref="ByteResponse"/> containing the response bytes.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown if the underlying request fails due to connection, protocol, or data processing errors.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the HTTP method on the request is not supported by this handler.
        /// </exception>
        public async Awaitable<ByteResponse> Send()
        {
            byte[] response = _request.method switch
            {
                HttpMethod.Get => await RequestHandler.CreateByteRequest(
                    method: HttpMethod.Get,
                    url:_request.url,
                    timeout: _request.timeout,
                    payload: null,
                    headers: _request.headers,
                    errorHandler: _request.errorHandler,
                    cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

                HttpMethod.Post => await RequestHandler.CreateByteRequest(
                    HttpMethod.Post,
                    _request.url,
                    timeout: _request.timeout,
                    payload: _request.transformer?.Invoke(_request.body),
                    headers: _request.headers,
                    errorHandler: _request.errorHandler,
                    cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

                _ => throw new InvalidOperationException("Invalid HTTP method.")
            };

            return new ByteResponse(response);
        }
    }
}