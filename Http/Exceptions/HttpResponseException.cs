#nullable enable

using System;
using System.Net;

namespace Http
{
    /// <summary>
    /// Represents an exception that occurs during an HTTP request,
    /// containing the status code and the response content.
    /// </summary>
    public class HttpResponseException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code of the response as a long.
        /// </summary>
        public long statusCode { get; }

        /// <summary>
        /// Gets the raw string content of the error response.
        /// </summary>
        public string? content { get; }

        /// <summary>
        /// Gets the HTTP status code of the response as a <see cref="HttpStatusCode"/> enum.
        /// </summary>
        public HttpStatusCode httpStatus => (HttpStatusCode)statusCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code from the response.</param>
        /// <param name="content">The raw content from the response body.</param>
        public HttpResponseException(long statusCode, string? content)
            : base($"Request failed with status code {statusCode}. Response: {content}")
        {
            this.statusCode = statusCode;
            this.content = content;
        }
    }
}