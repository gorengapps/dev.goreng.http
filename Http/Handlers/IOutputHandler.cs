using UnityEngine;

namespace Http.Handlers
{
    /// <summary>
    /// Interface that defines output handlers for different response types.
    /// Output handlers are responsible for processing HTTP requests and transforming
    /// the response data into the appropriate response type.
    /// </summary>
    /// <typeparam name="T">The type of response this handler produces.</typeparam>
    public interface IOutputHandler<T>
    {
        /// <summary>
        /// Sends the HTTP request and returns the response data as the specified type.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the typed response.</returns>
        public Awaitable<T> Send();
    }
}