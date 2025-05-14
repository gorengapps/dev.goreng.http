using System;

namespace Http
{
    public interface IHttpErrorHandler
    {
        /// <summary>
        /// Handles the error
        /// </summary>
        /// <param name="text"></param>
        public Exception HandleError(string text);
    }
}