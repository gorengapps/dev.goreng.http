namespace Http
{
    /// <summary>
    /// Define specific response types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface HttpResponse<out T>
    {
        public T rawResponse { get; }
    }
}