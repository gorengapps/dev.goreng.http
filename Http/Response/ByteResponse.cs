namespace Http
{
    /// <summary>
    /// Represents an HTTP response containing binary data.
    /// Use this response type when working with files, images, or other binary content.
    /// </summary>
    public class ByteResponse: HttpResponse<byte[]>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteResponse"/> class.
        /// </summary>
        /// <param name="rawResponse">The binary data from the HTTP response.</param>
        public ByteResponse(byte[] rawResponse)
        {
            this.rawResponse = rawResponse;
        }
        
        /// <summary>
        /// Gets the raw binary data from the HTTP response.
        /// </summary>
        public byte[] rawResponse { get; }
    }
}