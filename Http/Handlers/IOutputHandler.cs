using System.Threading.Tasks;

namespace Http.Handlers
{
    /// <summary>
    /// Interface allows you to define different type of outputs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOutputHandler<T>
    {
        /// <summary>
        /// Sends the request and returns the specific type
        /// </summary>
        /// <returns></returns>
        public Task<T> Send();
    }
}