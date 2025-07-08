namespace Http
{
    /// <summary>
    /// Bytes response that includes progress and size information
    /// </summary>
    public class ProgressByteResponse: HttpResponse<byte[]>
    {
        public ProgressByteResponse(byte[] rawResponse, ulong totalBytes)
        {
            this.rawResponse = rawResponse;
            TotalBytes = totalBytes;
            DownloadedBytes = (ulong)rawResponse.Length;
        }
        
        public byte[] rawResponse { get; }
        
        /// <summary>
        /// The total size of the downloaded content in bytes
        /// </summary>
        public ulong TotalBytes { get; }
        
        /// <summary>
        /// The number of bytes that were downloaded
        /// </summary>
        public ulong DownloadedBytes { get; }
        
        /// <summary>
        /// Gets the final download progress information
        /// </summary>
        public DownloadProgress Progress => new DownloadProgress(DownloadedBytes, TotalBytes);
    }
}