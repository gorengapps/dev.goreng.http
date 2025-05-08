#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
        /// <param name="payload">Optional JSON or other string payload to include in the request body.</param>
        /// <param name="headers">Optional per-request headers to apply in addition to the defaults.</param>
        /// <returns>
        /// A <see cref="DownloadHandler"/> containing the response data when the request succeeds.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown if the request results in a connection error, protocol error, or data processing error.
        /// </exception>
        private static async Awaitable<DownloadHandler> CreateRequest(
            HttpMethod method,
            string url,
            string? payload = null,
            Dictionary<string, string>? headers = null)
        {
            var request = new UnityWebRequest(url, method.ToString().ToUpper());
            request.downloadHandler = new DownloadHandlerBuffer();

            if (payload != null)
            {
                // Ensure the upload handler is disposed of along with the request.
                request.disposeCertificateHandlerOnDispose = true;
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
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
            
            // Send the request and await completion.
            await request.SendWebRequest();
            
            return request.result switch
            {
                UnityWebRequest.Result.Success => request.downloadHandler,
                UnityWebRequest.Result.ConnectionError
                or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError
                    => throw new HttpRequestException(request.error),
                _ => throw new HttpRequestException("Unexpected UnityWebRequest result")
            };
        }

        /// <summary>
        /// Sends an HTTP request expecting a binary response.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="payload">Optional request body.</param>
        /// <param name="headers">Optional additional headers.</param>
        /// <returns>A byte array containing the raw response data.</returns>
        /// <exception cref="HttpRequestException">On request failure.</exception>
        internal static async Awaitable<byte[]> CreateByteRequest(
            HttpMethod method,
            string url,
            string? payload,
            Dictionary<string, string>? headers = null)
        {
            var handler = await CreateRequest(method, url, payload, headers);
            return handler.data;
        }

        /// <summary>
        /// Sends an HTTP request expecting a text response, without a request body.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="headers">Optional additional headers.</param>
        /// <returns>The response body as a string.</returns>
        /// <exception cref="HttpRequestException">On request failure.</exception>
        internal static async Awaitable<string> CreateStringRequest(
            HttpMethod method,
            string url,
            Dictionary<string, string>? headers = null)
        {
            var handler = await CreateRequest(method, url, null, headers);
            return handler.text;
        }

        /// <summary>
        /// Sends an HTTP request with a string payload and returns the response text.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The target URL.</param>
        /// <param name="payload">The request body to send.</param>
        /// <param name="headers">Optional additional headers.</param>
        /// <returns>The response body as a string.</returns>
        /// <exception cref="HttpRequestException">On request failure.</exception>
        internal static async Awaitable<string> CreatePayloadRequest(
            HttpMethod method,
            string url,
            string? payload,
            Dictionary<string, string>? headers = null)
        {
            var handler = await CreateRequest(method, url, payload, headers);
            return handler.text;
        }
    }
}
