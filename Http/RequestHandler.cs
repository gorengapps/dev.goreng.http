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
    /// <summary>
    /// Internal class responsible for handling HTTP requests using UnityWebRequest.
    /// Provides methods for both standard requests and progress-aware requests.
    /// </summary>
    internal static class RequestHandler
    {
        /// <summary>
        /// Creates and sends an HTTP request without progress tracking.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="timeout">Optional timeout in seconds.</param>
        /// <param name="payload">Optional request body payload.</param>
        /// <param name="headers">Optional headers to include.</param>
        /// <param name="errorHandler">Optional custom error handler.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The download handler containing the response data.</returns>
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
        
        /// <summary>
        /// Creates and sends an HTTP request that returns binary data without progress tracking.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="timeout">Optional timeout in seconds.</param>
        /// <param name="payload">Optional request body payload.</param>
        /// <param name="headers">Optional headers to include.</param>
        /// <param name="errorHandler">Optional custom error handler.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The response data as a byte array.</returns>
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

        /// <summary>
        /// Creates and sends an HTTP GET request that returns string data without progress tracking.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="timeout">Optional timeout in seconds.</param>
        /// <param name="headers">Optional headers to include.</param>
        /// <param name="errorHandler">Optional custom error handler.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The response data as a string.</returns>
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

        /// <summary>
        /// Creates and sends an HTTP request with a payload that returns string data without progress tracking.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="timeout">Optional timeout in seconds.</param>
        /// <param name="payload">The request body payload.</param>
        /// <param name="headers">Optional headers to include.</param>
        /// <param name="errorHandler">Optional custom error handler.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The response data as a string.</returns>
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

        /// <summary>
        /// Creates and sends an HTTP request with progress tracking capabilities.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="timeout">Optional timeout in seconds.</param>
        /// <param name="payload">Optional request body payload.</param>
        /// <param name="headers">Optional headers to include.</param>
        /// <param name="errorHandler">Optional custom error handler.</param>
        /// <param name="progressCallback">Optional callback for progress updates.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The download handler containing the response data.</returns>
        private static async Awaitable<DownloadHandler> CreateRequestWithProgress(
            HttpMethod method,
            string url,
            int? timeout = null,
            string? payload = null,
            Dictionary<string, string>? headers = null,
            IHttpErrorHandler? errorHandler = null,
            ProgressCallback? progressCallback = null,
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
                
                // Poll for progress if callback is provided
                if (progressCallback != null)
                {
                    while (!op.isDone && !cancellationToken.IsCancellationRequested)
                    {
                        if(!long.TryParse(request.GetResponseHeader("Content-Length"), out var length))
                        {
                            continue;
                        }
                        
                        var totalBytes = length >= 0 ? (ulong)length : 0ul;
                        var downloadedBytes = request.downloadedBytes;
                        
                        progressCallback(new DownloadProgress(downloadedBytes, totalBytes));
                        await Awaitable.NextFrameAsync(cancellationToken);
                    }
                }
                
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

        /// <summary>
        /// Creates and sends an HTTP request that returns binary data with progress tracking.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="timeout">Optional timeout in seconds.</param>
        /// <param name="payload">Optional request body payload.</param>
        /// <param name="headers">Optional headers to include.</param>
        /// <param name="errorHandler">Optional custom error handler.</param>
        /// <param name="progressCallback">Optional callback for progress updates.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The response data as a byte array.</returns>
        internal static async Awaitable<byte[]> CreateByteRequestWithProgress(
            HttpMethod method,
            string url,
            int? timeout,
            string? payload,
            Dictionary<string, string>? headers = null,
            IHttpErrorHandler? errorHandler = null,
            ProgressCallback? progressCallback = null,
            CancellationToken cancellationToken = default)
        {
            var handler = await CreateRequestWithProgress(method, url, timeout, payload, headers, errorHandler, progressCallback, cancellationToken);
            return handler.data;
        }

        /// <summary>
        /// Creates and sends an HTTP GET request that returns string data with progress tracking.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="timeout">Optional timeout in seconds.</param>
        /// <param name="headers">Optional headers to include.</param>
        /// <param name="errorHandler">Optional custom error handler.</param>
        /// <param name="progressCallback">Optional callback for progress updates.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The response data as a string.</returns>
        internal static async Awaitable<string> CreateStringRequestWithProgress(
            HttpMethod method,
            string url,
            int? timeout,
            Dictionary<string, string>? headers = null,
            IHttpErrorHandler? errorHandler = null,
            ProgressCallback? progressCallback = null,
            CancellationToken cancellationToken = default)
        {
            var handler = await CreateRequestWithProgress(method, url, timeout, null, headers, errorHandler, progressCallback, cancellationToken);
            return handler.text;
        }

        /// <summary>
        /// Creates and sends an HTTP request with a payload that returns string data with progress tracking.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="timeout">Optional timeout in seconds.</param>
        /// <param name="payload">The request body payload.</param>
        /// <param name="headers">Optional headers to include.</param>
        /// <param name="errorHandler">Optional custom error handler.</param>
        /// <param name="progressCallback">Optional callback for progress updates.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The response data as a string.</returns>
        internal static async Awaitable<string> CreatePayloadRequestWithProgress(
            HttpMethod method,
            string url,
            int? timeout,
            string? payload,
            Dictionary<string, string>? headers = null,
            IHttpErrorHandler? errorHandler = null,
            ProgressCallback? progressCallback = null,
            CancellationToken cancellationToken = default)
        {
            var handler = await CreateRequestWithProgress(method, url, timeout, payload, headers, errorHandler, progressCallback, cancellationToken);
            return handler.text;
        }
    }
}