namespace Http
{
    /// <summary>
    /// Base interface for HTTP responses that contain typed data.
    /// Provides access to the raw response data of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of data contained in the response.</typeparam>
    public interface HttpResponse<out T>
    {
        /// <summary>
        /// Gets the raw response data of type T.
        /// </summary>
        public T rawResponse { get; }
    }
}