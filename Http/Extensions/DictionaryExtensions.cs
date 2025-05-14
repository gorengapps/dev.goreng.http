#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace Http.Extensions
{
    /// <summary>
    /// Converts all public instance fields of the specified object into a
    /// <see cref="Dictionary{TKey, TValue}"/>, using any [JsonProperty] name if present.
    /// </summary>
    internal static class DictionaryExtensions
    {
        internal static Dictionary<string, string> ToDictionary(this object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var result = new Dictionary<string, string>();
            var fields = obj.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (var field in fields)
            {
                var rawValue = field.GetValue(obj);
                
                if (rawValue == null)
                    continue;
                
                var key = field.Name;
                
                var jsonProp = field.GetCustomAttribute<JsonPropertyAttribute>();
                
                if (jsonProp != null && !string.IsNullOrWhiteSpace(jsonProp.PropertyName))
                {
                    key = jsonProp.PropertyName!;
                }

                try
                {
                    result[key] = rawValue.ToString()!;
                }
                catch
                {
                    // SKIP
                }
            }

            return result;
        }
    }
}