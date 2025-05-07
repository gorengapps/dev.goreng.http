#nullable enable

using System;
using System.Linq;
using Http.Extensions;

namespace Http.Transformers
{
    /// <summary>
    /// Provides a transformer that serializes an object's public instance fields
    /// into an application/x-www-form-urlencoded string.
    /// </summary>
    public static class FormEncodedTransformer
    {
        /// <summary>
        /// Transforms the specified object's public instance fields into a URL-encoded form data string.
        /// </summary>
        /// <param name="obj">
        /// The object whose fields will be serialized into key-value pairs.
        /// </param>
        /// <returns>
        /// A string in the format "key1=value1&key2=value2...", where each key is a field name
        /// and each value is the corresponding field's string representation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="obj"/> is <c>null</c>.
        /// </exception>
        public static string Transform(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var kvp = obj.ToDictionary()
                .Select(x => $"{x.Key}={x.Value}");

            return string.Join(
                "&", 
                kvp
            );
        }
    }
}