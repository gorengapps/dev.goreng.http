#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace Http.Handlers
{
    public class OutputByteHandler : IOutputHandler<ByteResponse>
    {
        private readonly Request _request;

        public OutputByteHandler(Request request)
        {
            _request = request;
        }

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
                        cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

                    HttpMethod.Post => await RequestHandler.CreateByteRequestWithProgress(
                        HttpMethod.Post,
                        _request.url,
                        timeout: _request.timeout,
                        payload: _request.transformer?.Invoke(_request.body),
                        headers: _request.GetAllHeaders(),
                        errorHandler: _request.errorHandler,
                        progressCallback: _request.progressCallback,
                        cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

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
                        cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

                    HttpMethod.Post => await RequestHandler.CreateByteRequest(
                        HttpMethod.Post,
                        _request.url,
                        timeout: _request.timeout,
                        payload: _request.transformer?.Invoke(_request.body),
                        headers: _request.GetAllHeaders(),
                        errorHandler: _request.errorHandler,
                        cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

                    _ => throw new InvalidOperationException("Invalid HTTP method.")
                };
            }

            return new ByteResponse(response);
        }
    }
}