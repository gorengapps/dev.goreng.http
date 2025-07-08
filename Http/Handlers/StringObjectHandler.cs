#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace Http.Handlers
{
    /// <summary>
    /// Output handler that processes HTTP requests and returns string data as a <see cref="StringResponse"/>.
    /// Supports both standard requests and progress-tracked requests.
    /// </summary>
    public class StringObjectHandler : IOutputHandler<StringResponse>
    {
        private readonly Request _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringObjectHandler"/> class.
        /// </summary>
        /// <param name="request">The request to process.</param>
        public StringObjectHandler(Request request)
        {
            _request = request;
        }

        /// <summary>
        /// Sends the HTTP request and returns the response as string data.
        /// Automatically uses progress tracking if a progress callback is configured.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the string response.</returns>
        public async Awaitable<StringResponse> Send()
        {
            string response;
            
            // Use progress-aware methods if progress callback is set
            if (_request.progressCallback != null)
            {
                response = _request.method switch
                {
                    HttpMethod.Get => await RequestHandler.CreateStringRequestWithProgress(
                        _request.method,
                        _request.url,
                        _request.timeout,
                        _request.GetAllHeaders(),
                        _request.errorHandler,
                        _request.progressCallback,
                        cancellationToken: _request.cancellationToken ?? CancellationToken.None),

                    HttpMethod.Post => await RequestHandler.CreatePayloadRequestWithProgress(
                        HttpMethod.Post,
                        _request.url,
                        _request.timeout,
                        _request.transformer?.Invoke(_request.body),
                        _request.GetAllHeaders(),
                        _request.errorHandler,
                        _request.progressCallback,
                        cancellationToken: _request.cancellationToken ?? CancellationToken.None),

                    _ => throw new InvalidOperationException("Invalid HTTP method.")
                };
            }
            else
            {
                response = _request.method switch
                {
                    HttpMethod.Get => await RequestHandler.CreateStringRequest(
                        _request.method,
                        _request.url,
                        _request.timeout,
                        _request.GetAllHeaders(),
                        _request.errorHandler,
                        cancellationToken: _request.cancellationToken ?? CancellationToken.None),

                    HttpMethod.Post => await RequestHandler.CreatePayloadRequest(
                        HttpMethod.Post,
                        _request.url,
                        _request.timeout,
                        _request.transformer?.Invoke(_request.body),
                        _request.GetAllHeaders(),
                        _request.errorHandler,
                        cancellationToken: _request.cancellationToken ?? CancellationToken.None),

                    _ => throw new InvalidOperationException("Invalid HTTP method.")
                };
            }

            return new StringResponse(response);
        }
    }
}