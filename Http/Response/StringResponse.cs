using Newtonsoft.Json;

namespace Http
{
    /// <summary>
    /// Represents an HTTP response containing string data.
    /// Use this response type when working with text content, JSON, XML, or other string-based data.
    /// </summary>
    public class StringResponse: HttpResponse<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringResponse"/> class.
        /// </summary>
        /// <param name="response">The string data from the HTTP response.</param>
        public StringResponse(string response)
        {
            rawResponse = response;
        }

        /// <summary>
        /// Deserializes the response string to the specified type using Newtonsoft.Json.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to.</typeparam>
        /// <returns>The deserialized object of type T.</returns>
        /// <exception cref="JsonException">Thrown when the response cannot be deserialized to the specified type.</exception>
        public T To<T>()
        {
            return JsonConvert.DeserializeObject<T>(rawResponse);
        }

        /// <summary>
        /// Gets the raw string data from the HTTP response.
        /// </summary>
        public string rawResponse { get; }
    }
}