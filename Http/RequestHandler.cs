#nullable enable

using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Http
{
    internal static class RequestHandler
    {
        private static async Awaitable<DownloadHandler> CreateRequest(
            HttpMethod method,
            string url,
            int? timeout = null,
            string? payload = null,
            Dictionary<string, string>? headers = null,
            IHttpErrorHandler? errorHandler = null,
            CancellationToken cancellationToken = default)
        {
            var request = new UnityWebRequest(url, method.ToString().ToUpper());
            request.downloadHandler = new DownloadHandlerBuffer();

            if (payload != null)
            {
                request.disposeCertificateHandlerOnDispose = true;
                request.uploadHandler = new UploadHandlerRaw(new UTF8Encoding().GetBytes(payload));
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            request.timeout = timeout * 1000 ?? 30 * 1000;
            
            var op = request.SendWebRequest();

            await using (cancellationToken.Register(() => {
                request.Abort();
            }, useSynchronizationContext: false)) {
                await op;  
            }
            
            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException($"Request to {url} was canceled.");
            }

            return request.result switch
            {
                UnityWebRequest.Result.Success => request.downloadHandler,
                UnityWebRequest.Result.ConnectionError 
                or UnityWebRequest.Result.ProtocolError 
                or UnityWebRequest.Result.DataProcessingError 
                    => throw errorHandler?.HandleError(request) ?? new HttpResponseException(request.responseCode, request.downloadHandler.text),
                _ => throw new HttpRequestException("Unexpected UnityWebRequest result")
            };
        }
        
        internal static async Awaitable<byte[]> CreateByteRequest(
            HttpMethod method,
            string url,
            int? timeout,
            string? payload,
            Dictionary<string, string>? headers = null,
            IHttpErrorHandler? errorHandler = null,
            CancellationToken cancellationToken = default)
        {
            var handler = await CreateRequest(method, url, timeout, payload, headers, errorHandler, cancellationToken);
            return handler.data;
        }

        internal static async Awaitable<string> CreateStringRequest(
            HttpMethod method,
            string url,
            int? timeout,
            Dictionary<string, string>? headers = null,
            IHttpErrorHandler? errorHandler = null,
            CancellationToken cancellationToken = default)
        {
            var handler = await CreateRequest(method, url, timeout, null, headers, errorHandler, cancellationToken);
            return handler.text;
        }

        internal static async Awaitable<string> CreatePayloadRequest(
            HttpMethod method,
            string url,
            int? timeout,
            string? payload,
            Dictionary<string, string>? headers = null,
            IHttpErrorHandler? errorHandler = null,
            CancellationToken cancellationToken = default)
        {
            var handler = await CreateRequest(method, url, timeout, payload, headers, errorHandler, cancellationToken);
            return handler.text;
        }
    }
}