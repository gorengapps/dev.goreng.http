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
    /// Provides a set of helper methods to send HTTP requests using UnityWebRequest,
    /// with support for default headers, optional payloads, and error handling.
    /// </summary>
    internal static class RequestHandler
    {
        /// <summary>
        /// Headers that will be applied to every request by default.
        /// </summary>
        internal static readonly Dictionary<string, string> defaultHeaders = new Dictionary<string, string>();

        /// <summary>
        /// Creates and sends an HTTP request with the given method, URL, optional payload, and headers.
        /// </summary>
        /// <param name="method">The HTTP method to use (GET, POST, etc.).</param>
        /// <param name="url">The full URL to which the request will be sent.</param>
        /// <param name="timeout"></param>
        /// <param name="payload">Optional JSON or other string payload to include in the request body.</param>
        /// <param name="headers">Optional per-request headers to apply in addition to the defaults.</param>
        /// <param name="errorHandler"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="DownloadHandler"/> containing the response data when the request succeeds.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown if the request results in a connection error, protocol error, or data processing error.
        /// </exception>
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
                // Ensure the upload handler is disposed of along with the request.
                request.disposeCertificateHandlerOnDispose = true;
                request.uploadHandler = new UploadHandlerRaw(new UTF8Encoding().GetBytes(payload));
            }

            // Merge default headers with any per-request headers.
            var appliedHeaders = headers ?? new Dictionary<string, string>();
            
            foreach (var header in defaultHeaders)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            foreach (var header in appliedHeaders)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            request.timeout = timeout * 1000 ?? 30 * 1000;
            
            // kick off the request
            var op = request.SendWebRequest();

            // if the token fires, abort the UnityWebRequest
            await using (cancellationToken.Register(() => {
                request.Abort();
            }, useSynchronizationContext: false)) {
                await op;  
            }
            
            // if we were cancelled, Unity will set result to Error and error to "Request aborted"
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
                    => throw errorHandler?.HandleError(request.downloadHandler.text) ?? new HttpRequestException(request.downloadHandler.text),
                _ => throw new HttpRequestException("Unexpected UnityWebRequest result")
            };
        }

        /// <summary>
        /// Sends an HTTP request expecting a binary response.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="timeout">The timeout of the request</param>
        /// <param name="payload">Optional request body.</param>
        /// <param name="headers">Optional additional headers.</param>
        /// <param name="errorHandler"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A byte array containing the raw response data.</returns>
        /// <exception cref="HttpRequestException">On request failure.</exception>
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
        /// Sends an HTTP request expecting a text response, without a request body.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="timeout">The timeout of the request</param>
        /// <param name="headers">Optional additional headers.</param>
        /// <param name="errorHandler"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The response body as a string.</returns>
        /// <exception cref="HttpRequestException">On request failure.</exception>
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
        /// Sends an HTTP request with a string payload and returns the response text.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="timeout">The timeout of the request</param>
        /// <param name="payload">The request body to send.</param>
        /// <param name="headers">Optional additional headers.</param>
        /// <param name="errorHandler"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The response body as a string.</returns>
        /// <exception cref="HttpRequestException">On request failure.</exception>
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
