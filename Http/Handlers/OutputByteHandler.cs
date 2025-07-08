#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace Http.Handlers
{
    /// <summary>
    /// Output handler that processes HTTP requests and returns binary data as a <see cref="ByteResponse"/>.
    /// Supports both standard requests and progress-tracked requests.
    /// </summary>
    public class OutputByteHandler : IOutputHandler<ByteResponse>
    {
        private readonly Request _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputByteHandler"/> class.
        /// </summary>
        /// <param name="request">The request to process.</param>
        public OutputByteHandler(Request request)
        {
            _request = request;
        }

        /// <summary>
        /// Sends the HTTP request and returns the response as binary data.
        /// Automatically uses progress tracking if a progress callback is configured.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the binary response.</returns>
        public async Awaitable<ByteResponse> Send()
        {
            byte[] response;
            
            // Use progress-aware methods if progress callback is set
            if (_request.progressCallback != null)
            {
                response = _request.method switch
                {
                    HttpMethod.Get => await RequestHandler.CreateByteRequestWithProgress(
                        method: HttpMethod.Get,
                        url:_request.url,
                        timeout: _request.timeout,
                        payload: null,
                        headers: _request.GetAllHeaders(),
                        errorHandler: _request.errorHandler,
                        progressCallback: _request.progressCallback,
                        cancellationToken: _request.cancellationToken ?? CancellationToken.None),

                    HttpMethod.Post => await RequestHandler.CreateByteRequestWithProgress(
                        HttpMethod.Post,
                        _request.url,
                        timeout: _request.timeout,
                        payload: _request.transformer?.Invoke(_request.body),
                        headers: _request.GetAllHeaders(),
                        errorHandler: _request.errorHandler,
                        progressCallback: _request.progressCallback,
                        cancellationToken: _request.cancellationToken ?? CancellationToken.None),

                    _ => throw new InvalidOperationException("Invalid HTTP method.")
                };
            }
            else
            {
                response = _request.method switch
                {
                    HttpMethod.Get => await RequestHandler.CreateByteRequest(
                        method: HttpMethod.Get,
                        url:_request.url,
                        timeout: _request.timeout,
                        payload: null,
                        headers: _request.GetAllHeaders(),
                        errorHandler: _request.errorHandler,
                        cancellationToken: _request.cancellationToken ?? CancellationToken.None),

                    HttpMethod.Post => await RequestHandler.CreateByteRequest(
                        HttpMethod.Post,
                        _request.url,
                        timeout: _request.timeout,
                        payload: _request.transformer?.Invoke(_request.body),
                        headers: _request.GetAllHeaders(),
                        errorHandler: _request.errorHandler,
                        cancellationToken: _request.cancellationToken ?? CancellationToken.None),

                    _ => throw new InvalidOperationException("Invalid HTTP method.")
                };
            }

            return new ByteResponse(response);
        }
    }
}