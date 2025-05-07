using Newtonsoft.Json;

namespace Http
{
    /// <summary>
    /// String response is used to output the response as a string
    /// </summary>
    public class StringResponse: HttpResponse<string>
    {
        public StringResponse(string response)
        {
            rawResponse = response;
        }

        public T To<T>()
        {
            return JsonConvert.DeserializeObject<T>(rawResponse);
        }

        public string rawResponse { get; }
    }
}