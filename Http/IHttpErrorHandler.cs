using System;
using UnityEngine.Networking;

namespace Http
{
    /// <summary>
    /// Interface for custom HTTP error handling implementations.
    /// Allows applications to define custom logic for processing HTTP errors
    /// and creating appropriate exceptions based on the response.
    /// </summary>
    public interface IHttpErrorHandler
    {
        /// <summary>
        /// Handles the error response from an HTTP request and creates an appropriate exception.
        /// This method is called when an HTTP request fails with connection errors,
        /// protocol errors, or HTTP error status codes (4xx, 5xx).
        /// </summary>
        /// <param name="request">The failed UnityWebRequest object containing all response data including status code, headers, and error message.</param>
        /// <returns>An Exception to be thrown that represents the specific error condition.</returns>
        public Exception HandleError(UnityWebRequest request);
    }
}