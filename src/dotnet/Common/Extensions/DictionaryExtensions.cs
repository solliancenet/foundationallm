﻿using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Filters the keys of a dictionary based on a specified namespace.
        /// </summary>
        /// <param name="source">The dictionary to be filtered.</param>
        /// <param name="keyNamespace">The namespace of the keys to be returned in the resulting dictionary.</param>
        /// <returns>A new dictionary that contains the keys (and their values) which start with the provided namespace string.</returns>
        public static Dictionary<string, object> FilterKeys(this Dictionary<string, object> source, string keyNamespace) =>
            source
                .Where(x => x.Key.StartsWith(keyNamespace))
                .Select(x => new KeyValuePair<string, object>(
                    x.Key.Replace(keyNamespace, string.Empty),
                    x.Value is JsonElement element
                        ? element.ToObject()!
                        : x.Value))
                .ToDictionary(x => x.Key, x => x.Value);

        /// <summary>
        /// Converts a dictionary to an object of type <typeparamref name="T"/> with optional overrides.
        /// </summary>
        /// <typeparam name="T">The type of the object to which you want to convert from a source dictionary.</typeparam>
        /// <param name="source">The source properties for the returned object.</param>
        /// <param name="overrides">Optional overrides to apply to the returned object parameters.</param>
        /// <returns></returns>
        public static T ToObject<T>(this Dictionary<string, object> source, Dictionary<string, object>? overrides = null) where T : new()
        {
            T result = new T();
            var properties = typeof(T).GetProperties();

            // Helper method for conversion.
            object? ConvertToType(object value, Type targetType)
            {
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    if (value == null) return null;
                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                // Direct assignable check:
                if (targetType.IsInstanceOfType(value))
                {
                    return value;
                }

                // Handling enums:
                if (targetType.IsEnum)
                {
                    return Enum.Parse(targetType, value.ToString()!);
                }

                // Handling IConvertible types:
                try
                {
                    return Convert.ChangeType(value.ToString(), targetType);
                }
                catch (InvalidCastException)
                {
                    Console.WriteLine($"Cannot convert '{value}' to {targetType.Name}");
                    return null;
                }
            }

            // Apply main dictionary values.
            foreach (var item in source)
            {
                PropertyInfo? prop = FindProperty(properties, item.Key);
                if (prop != null && item.Value != null)
                {
                    var convertedValue = ConvertToType(item.Value, prop.PropertyType);
                    if (convertedValue != null || prop.PropertyType.IsNullableType())
                    {
                        prop.SetValue(result, convertedValue, null);
                    }
                }
            }

            // Apply overrides if any.
            if (overrides != null)
            {
                foreach (var item in overrides)
                {
                    PropertyInfo? prop = FindProperty(properties, item.Key);
                    if (prop != null && item.Value != null)
                    {
                        var convertedValue = ConvertToType(item.Value, prop.PropertyType);
                        if (convertedValue != null || prop.PropertyType.IsNullableType())
                        {
                            prop.SetValue(result, convertedValue, null);
                        }
                    }
                }
            }

            return result;
        }

        // Finds property by name or JsonPropertyName attribute.
        private static PropertyInfo? FindProperty(PropertyInfo[] properties, string key)
        {
            foreach (var prop in properties)
            {
                if (string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name, key, StringComparison.OrdinalIgnoreCase))
                {
                    return prop;
                }
            }
            return null;
        }

        // Extension method to check if a type is a nullable type.
        private static bool IsNullableType(this Type type) => Nullable.GetUnderlyingType(type) != null;
    }
}
