#nullable enable

namespace Http
{
    /// <summary>
    /// Represents download progress information.
    /// </summary>
    public struct DownloadProgress
    {
        /// <summary>
        /// The number of bytes downloaded so far.
        /// </summary>
        public ulong bytesDownloaded { get; }
        
        /// <summary>
        /// The total number of bytes to download. May be 0 if unknown.
        /// </summary>
        public ulong totalBytes { get; }
        
        /// <summary>
        /// The download progress as a percentage (0.0 to 1.0). 
        /// Returns 0 if total bytes is unknown.
        /// </summary>
        public float progress => totalBytes > 0 ? (float)bytesDownloaded / totalBytes : 0f;

        public DownloadProgress(ulong bytesDownloaded, ulong totalBytes)
        {
            this.bytesDownloaded = bytesDownloaded;
            this.totalBytes = totalBytes;
        }
    }

    /// <summary>
    /// Callback interface for receiving download progress updates.
    /// </summary>
    /// <param name="progress">The current download progress information.</param>
    public delegate void ProgressCallback(DownloadProgress progress);
}