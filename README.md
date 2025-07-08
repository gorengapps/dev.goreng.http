# dev.goreng.http

A simple wrapper around UnityWebRequest with support for progress tracking and file size information.

## Features

- Simple HTTP request wrapper for Unity
- Support for GET, POST requests
- Progress tracking for downloads
- File size information
- Cancellation token support
- Error handling

## Usage

### Basic Request
```csharp
var httpEngine = new HttpEngine();
var response = await httpEngine.Make("https://api.example.com/data")
    .SetMethod(HttpMethod.Get)
    .Send();
```

### Download with Progress Tracking
```csharp
var httpEngine = new HttpEngine();

// Progress tracking works with any output type
var request = httpEngine.Make("https://example.com/largefile.zip")
    .SetMethod(HttpMethod.Get)
    .SetProgressCallback(OnProgressUpdate);

// Use with byte output
var byteResponse = await request.SetByteOutput().Send();

// Or use with string output
var stringResponse = await request.SetStringOutput().Send();

// Or use with default string output
var defaultResponse = await request.Send();

private void OnProgressUpdate(DownloadProgress progress)
{
    if (progress.TotalBytes > 0)
    {
        Debug.Log($"Progress: {progress.Progress * 100:F1}%");
    }
    Debug.Log($"Downloaded: {progress.BytesDownloaded} bytes");
}
```

### Available Response Types
- `StringResponse`: For text-based responses
- `ByteResponse`: For binary data
