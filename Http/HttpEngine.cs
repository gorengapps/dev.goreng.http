#nullable enable

namespace Http
{
    /// <summary>
    /// Provides methods for creating <see cref="Request"/> instances and
    /// managing default headers applied to all HTTP requests.
    /// </summary>
    public class HttpEngine
    {
        /// <summary>
        /// Creates a new <see cref="Request"/> configured with the specified URL.
        /// </summary>
        /// <param name="url">The target URL for the request.</param>
        /// <returns>A new <see cref="Request"/> instance.</returns>
        public static Request Make(string url)
        {
            return new Request(url);
        }

        /// <summary>
        /// Adds a header that will be included by default in every request.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <param name="value">The value of the header.</param>
        public static void AddHeader(string key, string value)
        {
            RequestHandler.defaultHeaders[key] = value;    
        }
        
        /// <summary>
        /// Removes a previously added default header so it will not be sent in future requests.
        /// </summary>
        /// <param name="key">The name of the header to remove.</param>
        public static void RemoveHeader(string key)
        {
            RequestHandler.defaultHeaders.Remove(key);
        }
    }
}