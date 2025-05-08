#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Http.Extensions
{
    /// <summary>
    /// Provides extension methods for converting an object's public instance fields to a dictionary of string values.
    /// </summary>
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Converts all public instance fields of the specified object into a <see cref="Dictionary{TKey, TValue}"/>
        /// mapping field names to their string representations.
        /// </summary>
        /// <param name="obj">The object whose fields are to be converted.</param>
        /// <returns>
        /// A dictionary where each key is the name of a public instance field on <paramref name="obj"/>,
        /// and each value is the result of calling <c>ToString()</c> on that field's value.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is <c>null</c>.</exception>
        internal static Dictionary<string, string> ToDictionary(this object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var result = new Dictionary<string, string>();
            var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (var field in fields)
            {
                object? rawValue = field.GetValue(obj);
                if (rawValue == null)
                    continue;

                try
                {
                    // Use ToString() to get a string representation of the field's value
                    result[field.Name] = rawValue.ToString()!;
                }
                catch (Exception)
                {
                    // If ToString() throws or returns null, skip this field
                }
            }

            return result;
        }
    }
}