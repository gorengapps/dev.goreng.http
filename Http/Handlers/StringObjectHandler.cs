#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace Http.Handlers
{
    public class StringObjectHandler : IOutputHandler<StringResponse>
    {
        private readonly Request _request;

        public StringObjectHandler(Request request)
        {
            _request = request;
        }

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
                        cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

                    HttpMethod.Post => await RequestHandler.CreatePayloadRequestWithProgress(
                        HttpMethod.Post,
                        _request.url,
                        _request.timeout,
                        _request.transformer?.Invoke(_request.body),
                        _request.GetAllHeaders(),
                        _request.errorHandler,
                        _request.progressCallback,
                        cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

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
                        cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

                    HttpMethod.Post => await RequestHandler.CreatePayloadRequest(
                        HttpMethod.Post,
                        _request.url,
                        _request.timeout,
                        _request.transformer?.Invoke(_request.body),
                        _request.GetAllHeaders(),
                        _request.errorHandler,
                        cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

                    _ => throw new InvalidOperationException("Invalid HTTP method.")
                };
            }

            return new StringResponse(response);
        }
    }
}