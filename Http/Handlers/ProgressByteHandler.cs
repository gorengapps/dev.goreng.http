#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace Http.Handlers
{
    public class ProgressByteHandler : IOutputHandler<ProgressByteResponse>
    {
        private readonly Request _request;

        public ProgressByteHandler(Request request)
        {
            _request = request;
        }

        public async Awaitable<ProgressByteResponse> Send()
        {
            ulong totalBytes = 0;
            byte[] response = _request.method switch
            {
                HttpMethod.Get => await RequestHandler.CreateByteRequestWithProgress(
                    method: HttpMethod.Get,
                    url:_request.url,
                    timeout: _request.timeout,
                    payload: null,
                    headers: _request.GetAllHeaders(),
                    errorHandler: _request.errorHandler,
                    progressCallback: (progress) => {
                        totalBytes = progress.TotalBytes;
                        _request.progressCallback?.Invoke(progress);
                    },
                    cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

                HttpMethod.Post => await RequestHandler.CreateByteRequestWithProgress(
                    HttpMethod.Post,
                    _request.url,
                    timeout: _request.timeout,
                    payload: _request.transformer?.Invoke(_request.body),
                    headers: _request.GetAllHeaders(),
                    errorHandler: _request.errorHandler,
                    progressCallback: (progress) => {
                        totalBytes = progress.TotalBytes;
                        _request.progressCallback?.Invoke(progress);
                    },
                    cancellationToken: _request.cancellationToken?.Token ?? CancellationToken.None),

                _ => throw new InvalidOperationException("Invalid HTTP method.")
            };

            return new ProgressByteResponse(response, totalBytes);
        }
    }
}