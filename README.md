# dev.goreng.http

A comprehensive HTTP client wrapper for Unity built on top of UnityWebRequest, featuring progress tracking, file size information, and flexible response handling.

## Features

- 🚀 **Simple HTTP client** - Clean, fluent API for Unity projects
- 📊 **Progress tracking** - Real-time download progress callbacks for all request types
- 📏 **File size information** - Access to total and downloaded bytes when available
- 🔄 **Multiple response types** - String, byte array, and custom response handling
- ⏱️ **Cancellation support** - Full CancellationToken integration
- 🛡️ **Error handling** - Comprehensive error handling with custom error handlers
- 🔧 **Configurable** - Timeouts, headers, request methods, and transformers
- 🎯 **Type-safe** - Full nullable reference type support

## Quick Start

### Basic Usage
```csharp
var httpEngine = new HttpEngine();

// Simple GET request
var response = await httpEngine.Make("https://api.example.com/data").Send();
Console.WriteLine(response.rawResponse);

// POST request with JSON body
var postResponse = await httpEngine.Make("https://api.example.com/users")
    .SetMethod(HttpMethod.Post)
    .SetBody(new { name = "John", email = "john@example.com" })
    .Send();
```

### Working with Different Response Types

#### String Responses (Default)
```csharp
var response = await httpEngine.Make("https://api.example.com/data").Send();
string text = response.rawResponse;

// Parse JSON
var user = response.To<User>(); // Uses Newtonsoft.Json
```

#### Binary Data
```csharp
var response = await httpEngine.Make("https://example.com/file.pdf")
    .SetByteOutput()
    .Send();
byte[] data = response.rawResponse;
```

## Progress Tracking

Progress tracking works with **all request types** - no special handlers needed!

### Basic Progress Tracking
```csharp
var request = httpEngine.Make("https://example.com/largefile.zip")
    .SetProgressCallback(progress => {
        Debug.Log($"Downloaded: {progress.BytesDownloaded} bytes");
        
        if (progress.TotalBytes > 0) {
            Debug.Log($"Progress: {progress.Progress * 100:F1}%");
            Debug.Log($"Total size: {progress.TotalBytes} bytes");
        } else {
            Debug.Log("File size unknown - streaming download");
        }
    });

// Works with any response type
var response = await request.SetByteOutput().Send();
```

### Progress with UI Updates
```csharp
public class DownloadManager : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text statusText;

    public async Task DownloadFileAsync(string url)
    {
        var httpEngine = new HttpEngine();
        
        var response = await httpEngine.Make(url)
            .SetProgressCallback(OnDownloadProgress)
            .SetByteOutput()
            .Send();
            
        // File downloaded successfully
        File.WriteAllBytes("downloaded_file.bin", response.rawResponse);
    }

    private void OnDownloadProgress(DownloadProgress progress)
    {
        if (progress.TotalBytes > 0)
        {
            progressBar.value = progress.Progress;
            statusText.text = $"Downloading... {progress.Progress * 100:F1}%";
        }
        else
        {
            statusText.text = $"Downloaded: {progress.BytesDownloaded:N0} bytes";
        }
    }
}
```

## Advanced Configuration

### Headers and Authentication
```csharp
var httpEngine = new HttpEngine();

// Global headers for all requests
httpEngine.AddHeader("Authorization", "Bearer your-token");
httpEngine.AddHeader("User-Agent", "MyApp/1.0");

// Per-request headers
var response = await httpEngine.Make("https://api.example.com/protected")
    .SetHeader("X-Custom-Header", "value")
    .Send();
```

### Request Customization
```csharp
var response = await httpEngine.Make("https://api.example.com/data")
    .SetMethod(HttpMethod.Post)
    .SetTimeout(60) // 60 seconds
    .SetBody(requestData)
    .SetTransformer(obj => JsonConvert.SerializeObject(obj)) // Custom serialization
    .Send();
```

### Cancellation Support
```csharp
using var cts = new CancellationTokenSource();

// Cancel after 30 seconds
cts.CancelAfter(TimeSpan.FromSeconds(30));

try
{
    var response = await httpEngine.Make("https://api.example.com/slow-endpoint")
        .SetCancellationToken(cts.Token)
        .SetProgressCallback(progress => {
            // User can cancel during download
            if (userClickedCancel)
                cts.Cancel();
        })
        .Send();
}
catch (TaskCanceledException)
{
    Debug.Log("Request was cancelled");
}
```

### Error Handling
```csharp
public class CustomErrorHandler : IHttpErrorHandler
{
    public Exception HandleError(UnityWebRequest request)
    {
        return request.responseCode switch
        {
            404 => new FileNotFoundException($"Resource not found: {request.url}"),
            401 => new UnauthorizedAccessException("Authentication failed"),
            _ => new HttpRequestException($"HTTP {request.responseCode}: {request.error}")
        };
    }
}

var response = await httpEngine.Make("https://api.example.com/data")
    .SetErrorHandler(new CustomErrorHandler())
    .Send();
```

## API Reference

### HttpEngine Class

#### Methods
- **`Make(string url)`** - Creates a new request for the specified URL
- **`AddHeader(string key, string value)`** - Adds a default header for all requests
- **`RemoveHeader(string key)`** - Removes a default header

### Request Class

#### Configuration Methods
- **`SetMethod(HttpMethod method)`** - Sets the HTTP method (GET, POST, etc.)
- **`SetHeader(string key, string value)`** - Adds a header to this request
- **`SetTimeout(int seconds)`** - Sets the request timeout
- **`SetBody(object body)`** - Sets the request body (will be serialized)
- **`SetTransformer(Func<object?, string?> transformer)`** - Sets custom body serialization
- **`SetErrorHandler(IHttpErrorHandler errorHandler)`** - Sets custom error handling
- **`SetCancellationToken(CancellationToken token)`** - Enables request cancellation
- **`SetProgressCallback(ProgressCallback callback)`** - Enables progress tracking

#### Output Methods
- **`Send()`** - Sends request and returns StringResponse
- **`SetStringOutput().Send()`** - Explicitly returns StringResponse
- **`SetByteOutput().Send()`** - Returns ByteResponse for binary data

### Response Types

#### StringResponse
```csharp
public class StringResponse
{
    public string rawResponse { get; }
    public T To<T>() // Deserialize JSON to type T
}
```

#### ByteResponse
```csharp
public class ByteResponse
{
    public byte[] rawResponse { get; }
}
```

### Progress Tracking Types

#### DownloadProgress Struct
```csharp
public struct DownloadProgress
{
    public ulong BytesDownloaded { get; }  // Bytes downloaded so far
    public ulong TotalBytes { get; }       // Total bytes (0 if unknown)
    public float Progress { get; }         // Progress as 0.0-1.0 (0 if total unknown)
}
```

#### ProgressCallback Delegate
```csharp
public delegate void ProgressCallback(DownloadProgress progress);
```

## Examples

### Downloading Large Files with Progress
```csharp
public async Task<byte[]> DownloadWithProgressAsync(string url, IProgress<float> progress = null)
{
    var httpEngine = new HttpEngine();
    
    return await httpEngine.Make(url)
        .SetProgressCallback(downloadProgress => {
            if (downloadProgress.TotalBytes > 0)
            {
                progress?.Report(downloadProgress.Progress);
                Debug.Log($"Downloaded {downloadProgress.BytesDownloaded}/{downloadProgress.TotalBytes} bytes " +
                         $"({downloadProgress.Progress * 100:F1}%)");
            }
        })
        .SetByteOutput()
        .Send()
        .ContinueWith(task => task.Result.rawResponse);
}
```

### API Client Example
```csharp
public class ApiClient
{
    private readonly HttpEngine _httpEngine;
    
    public ApiClient(string baseUrl, string apiKey)
    {
        _httpEngine = new HttpEngine();
        _httpEngine.AddHeader("Authorization", $"Bearer {apiKey}");
        _httpEngine.AddHeader("Content-Type", "application/json");
    }
    
    public async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _httpEngine.Make($"{baseUrl}/{endpoint}")
            .SetMethod(HttpMethod.Get)
            .Send();
            
        return response.To<T>();
    }
    
    public async Task<T> PostAsync<T>(string endpoint, object data)
    {
        var response = await _httpEngine.Make($"{baseUrl}/{endpoint}")
            .SetMethod(HttpMethod.Post)
            .SetBody(data)
            .Send();
            
        return response.To<T>();
    }
}
```

### File Upload with Progress
```csharp
public async Task UploadFileAsync(string url, byte[] fileData, ProgressCallback onProgress = null)
{
    var httpEngine = new HttpEngine();
    
    var response = await httpEngine.Make(url)
        .SetMethod(HttpMethod.Post)
        .SetBody(fileData)
        .SetHeader("Content-Type", "application/octet-stream")
        .SetProgressCallback(onProgress)
        .Send();
        
    Debug.Log($"Upload completed: {response.rawResponse}");
}
```

## Performance Considerations

- **Progress Callbacks**: Called every frame during downloads. Avoid heavy operations in callbacks.
- **Large Files**: Use `SetByteOutput()` for files > 100MB to avoid string conversion overhead.
- **Memory**: Binary responses load entirely into memory. Consider streaming for very large files.
- **Threading**: All operations are async and return to the main thread via Unity's Awaitable system.

## Error Handling

The library throws different exceptions based on the error type:

- **`TaskCanceledException`** - Request was cancelled via CancellationToken
- **`HttpRequestException`** - Network or protocol errors
- **`HttpResponseException`** - HTTP error status codes (4xx, 5xx)
- Custom exceptions from your `IHttpErrorHandler`

```csharp
try
{
    var response = await httpEngine.Make("https://api.example.com/data").Send();
}
catch (TaskCanceledException)
{
    // Handle cancellation
}
catch (HttpResponseException ex) when (ex.StatusCode == 404)
{
    // Handle not found
}
catch (HttpRequestException ex)
{
    // Handle network errors
}
```

## Requirements

- Unity 2023.1 or later (for Awaitable support)
- Newtonsoft.Json package (for JSON serialization)

## Thread Safety

- `HttpEngine` instances are thread-safe for header management
- `Request` instances should not be shared between threads
- Progress callbacks are invoked on the main thread
