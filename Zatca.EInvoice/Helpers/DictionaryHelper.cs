using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace Zatca.EInvoice.Helpers
{
    /// <summary>
    /// Helper class for safely extracting values from dictionaries.
    /// </summary>
    public static class DictionaryHelper
    {
        /// <summary>
        /// Gets a string value from the dictionary, returning a default if not found.
        /// </summary>
        /// <param name="data">The dictionary to read from.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="defaultValue">The default value if key is not found.</param>
        /// <returns>The string value or default.</returns>
        public static string? GetString(Dictionary<string, object>? data, string key, string? defaultValue = null)
        {
            if (data == null || !data.TryGetValue(key, out var value) || value == null)
                return defaultValue;

            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind == JsonValueKind.Null
                    ? defaultValue
                    : jsonElement.GetString() ?? defaultValue;
            }

            return value.ToString() ?? defaultValue;
        }

        /// <summary>
        /// Gets a decimal value from the dictionary, returning a default if not found.
        /// </summary>
        /// <param name="data">The dictionary to read from.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="defaultValue">The default value if key is not found.</param>
        /// <returns>The decimal value or default.</returns>
        public static decimal GetDecimal(Dictionary<string, object>? data, string key, decimal defaultValue = 0m)
        {
            if (data == null || !data.TryGetValue(key, out var value) || value == null)
                return defaultValue;

            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.Number => jsonElement.GetDecimal(),
                    JsonValueKind.String when decimal.TryParse(jsonElement.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) => result,
                    _ => defaultValue
                };
            }

            if (value is decimal decimalValue)
                return decimalValue;

            if (value is double doubleValue)
                return (decimal)doubleValue;

            if (value is float floatValue)
                return (decimal)floatValue;

            if (value is int intValue)
                return intValue;

            if (value is long longValue)
                return longValue;

            if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                return parsed;

            return defaultValue;
        }

        /// <summary>
        /// Gets a boolean value from the dictionary, returning a default if not found.
        /// </summary>
        /// <param name="data">The dictionary to read from.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="defaultValue">The default value if key is not found.</param>
        /// <returns>The boolean value or default.</returns>
        public static bool GetBoolean(Dictionary<string, object>? data, string key, bool defaultValue = false)
        {
            if (data == null || !data.TryGetValue(key, out var value) || value == null)
                return defaultValue;

            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.String when bool.TryParse(jsonElement.GetString(), out var result) => result,
                    _ => defaultValue
                };
            }

            if (value is bool boolValue)
                return boolValue;

            if (bool.TryParse(value.ToString(), out var parsed))
                return parsed;

            return defaultValue;
        }

        /// <summary>
        /// Gets an integer value from the dictionary, returning a default if not found.
        /// </summary>
        /// <param name="data">The dictionary to read from.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="defaultValue">The default value if key is not found.</param>
        /// <returns>The integer value or default.</returns>
        public static int GetInt(Dictionary<string, object>? data, string key, int defaultValue = 0)
        {
            if (data == null || !data.TryGetValue(key, out var value) || value == null)
                return defaultValue;

            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.Number => jsonElement.GetInt32(),
                    JsonValueKind.String when int.TryParse(jsonElement.GetString(), out var result) => result,
                    _ => defaultValue
                };
            }

            if (value is int intValue)
                return intValue;

            if (value is long longValue)
                return (int)longValue;

            if (int.TryParse(value.ToString(), out var parsed))
                return parsed;

            return defaultValue;
        }

        /// <summary>
        /// Gets a nested dictionary from the dictionary, returning an empty dictionary if not found.
        /// </summary>
        /// <param name="data">The dictionary to read from.</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>The nested dictionary or an empty dictionary.</returns>
        public static Dictionary<string, object> GetDictionary(Dictionary<string, object>? data, string key)
        {
            if (data == null || !data.TryGetValue(key, out var value) || value == null)
                return new Dictionary<string, object>();

            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
            {
                var result = new Dictionary<string, object>();
                foreach (var property in jsonElement.EnumerateObject())
                {
                    result[property.Name] = property.Value;
                }
                return result;
            }

            if (value is Dictionary<string, object> dictValue)
                return dictValue;

            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets a list from the dictionary, returning null if not found.
        /// </summary>
        /// <param name="data">The dictionary to read from.</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>The list or null.</returns>
        public static IEnumerable<object>? GetList(Dictionary<string, object>? data, string key)
        {
            if (data == null || !data.TryGetValue(key, out var value) || value == null)
                return null;

            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                return ConvertJsonArrayToList(jsonElement);
            }

            if (value is IEnumerable<object> enumerable)
                return enumerable;

            return null;
        }

        private static List<object> ConvertJsonArrayToList(JsonElement jsonElement)
        {
            var result = new List<object>();
            foreach (var item in jsonElement.EnumerateArray())
            {
                result.Add(item.ValueKind == JsonValueKind.Object
                    ? ConvertJsonObjectToDictionary(item)
                    : item);
            }
            return result;
        }

        private static Dictionary<string, object> ConvertJsonObjectToDictionary(JsonElement item)
        {
            var dict = new Dictionary<string, object>();
            foreach (var prop in item.EnumerateObject())
            {
                dict[prop.Name] = prop.Value;
            }
            return dict;
        }
    }
}
