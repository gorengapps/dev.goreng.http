#nullable enable

using System.Collections.Generic;

namespace Http
{
    /// <summary>
    /// Provides methods for creating <see cref="Request"/> instances and
    /// managing default headers applied to all HTTP requests created by this engine.
    /// </summary>
    public class HttpEngine
    {
        private readonly Dictionary<string, string> _defaultHeaders = new Dictionary<string, string>();

        /// <summary>
        /// Creates a new <see cref="Request"/> configured with the specified URL
        /// and the default headers from this engine instance.
        /// </summary>
        /// <param name="url">The target URL for the request.</param>
        /// <returns>A new <see cref="Request"/> instance.</returns>
        public Request Make(string url)
        {
            return new Request(url, _defaultHeaders);
        }

        /// <summary>
        /// Adds a header that will be included by default in every request made by this engine.
        /// If the header already exists, its value will be updated.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <param name="value">The value of the header.</param>
        public void AddHeader(string key, string value)
        {
            _defaultHeaders[key] = value;
        }

        /// <summary>
        /// Removes a previously added default header so it will not be sent in future requests from this engine.
        /// </summary>
        /// <param name="key">The name of the header to remove.</param>
        public void RemoveHeader(string key)
        {
            _defaultHeaders.Remove(key);
        }
    }
}