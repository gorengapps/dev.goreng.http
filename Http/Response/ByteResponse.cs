namespace Http
{
    /// <summary>
    /// Bytes response is used to output the data as a byte array
    /// </summary>
    public class ByteResponse: HttpResponse<byte[]>
    {
        public ByteResponse(byte[] rawResponse)
        {
            this.rawResponse = rawResponse;
        }
        
        public byte[] rawResponse { get; }
    }
}