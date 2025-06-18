using System;
using UnityEngine.Networking;

namespace Http
{
    public interface IHttpErrorHandler
    {
        /// <summary>
        /// Handles the error response from an HTTP request.
        /// </summary>
        /// <param name="request">The failed UnityWebRequest object containing all response data.</param>
        /// <returns>An Exception to be thrown.</returns>
        public Exception HandleError(UnityWebRequest request);
    }
}