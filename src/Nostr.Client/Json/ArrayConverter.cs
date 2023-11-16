﻿using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Nostr.Client.Json
{
    /// <summary>
    /// Converter for arrays to objects. Can deserialize data like [0.1, 0.2, "test"] to an object. Mapping is done by marking the class with [JsonConverter(typeof(ArrayConverter))] and the properties
    /// with [ArrayProperty(x)] where x is the index of the property in the array
    /// </summary>
    public class ArrayConverter : JsonConverter
    {
        private static readonly ConcurrentDictionary<(MemberInfo, Type), Attribute?> AttributeByMemberInfoAndTypeCache = new();
        private static readonly ConcurrentDictionary<(Type, Type), Attribute?> AttributeByTypeAndTypeCache = new();

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
                return;

            writer.WriteStartArray();
            var props = value.GetType().GetProperties();
            var ordered = props.OrderBy(p => GetCustomAttribute<ArrayPropertyAttribute>(p)?.Index);

            var last = -1;
            foreach (var prop in ordered)
            {
                var arrayProp = GetCustomAttribute<ArrayPropertyAttribute>(prop);
                if (arrayProp == null)
                    continue;

                if (arrayProp.Index == last)
                    continue;

                while (arrayProp.Index != last + 1)
                {
                    writer.WriteValue((string?)null);
                    last += 1;
                }

                last = arrayProp.Index;
                var converterAttribute = GetCustomAttribute<JsonConverterAttribute>(prop);
                if (converterAttribute != null)
                    writer.WriteRawValue(
                        JsonConvert.SerializeObject(prop.GetValue(value),
                        (JsonConverter?)Activator.CreateInstance(converterAttribute.ConverterType) ?? throw new InvalidOperationException("Cannot create JsonConverter")));
                else if (!IsSimple(prop.PropertyType))
                    NostrSerializer.Serializer.Serialize(writer, prop.GetValue(value));
                else
                    writer.WriteValue(prop.GetValue(value));
            }

            if (value is IHaveAdditionalData valueWithData)
            {
                foreach (var additional in valueWithData.AdditionalData)
                {
                    if (!IsSimple(additional.GetType()))
                        NostrSerializer.Serializer.Serialize(writer, additional);
                    else
                        writer.WriteValue(additional);
                }
            }
            
            if (value is IHaveAdditionalStringData valueWithStringData)
            {
                foreach (var additional in valueWithStringData.AdditionalData)
                {
                    writer.WriteValue(additional);
                }
            }

            writer.WriteEndArray();
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (objectType == typeof(JToken))
                return JToken.Load(reader);

            var result = Activator.CreateInstance(objectType, true);
            var arr = JArray.Load(reader);
            return ParseObject(arr, result, objectType);
        }

        private static object? ParseObject(JArray arr, object? result, Type objectType)
        {
            var maxIndex = 0;
            foreach (var property in objectType.GetProperties())
            {
                var attribute = GetCustomAttribute<ArrayPropertyAttribute>(property);

                if (attribute == null)
                    continue;

                if (attribute.Index >= arr.Count)
                    continue;

                maxIndex++;

                if (property.PropertyType.BaseType == typeof(Array))
                {
                    var objType = property.PropertyType.GetElementType();
                    var innerArray = (JArray)arr[attribute.Index];
                    var count = 0;
                    if (innerArray.Count == 0)
                    {
                        var arrayResult = (IList?)Activator.CreateInstance(property.PropertyType, new[] { 0 });
                        property.SetValue(result, arrayResult);
                    }
                    else if (innerArray[0].Type == JTokenType.Array)
                    {
                        var arrayResult = (IList?)Activator.CreateInstance(property.PropertyType, new[] { innerArray.Count });
                        if (arrayResult == null)
                            throw new InvalidOperationException("Property cannot be cast to 'IList'");

                        foreach (var obj in innerArray)
                        {
                            var innerObj = Activator.CreateInstance(objType!);
                            arrayResult[count] = ParseObject((JArray)obj, innerObj, objType!);
                            count++;
                        }
                        property.SetValue(result, arrayResult);
                    }
                    else
                    {
                        var arrayResult = (IList?)Activator.CreateInstance(property.PropertyType, new[] { 1 });
                        if (arrayResult == null)
                            throw new InvalidOperationException("Property cannot be cast to 'IList'");

                        var innerObj = Activator.CreateInstance(objType!);
                        arrayResult[0] = ParseObject(innerArray, innerObj, objType!);
                        property.SetValue(result, arrayResult);
                    }
                    continue;
                }

                var converterAttribute = GetCustomAttribute<JsonConverterAttribute>(property) ?? GetCustomAttribute<JsonConverterAttribute>(property.PropertyType);
                var conversionAttribute = GetCustomAttribute<JsonConversionAttribute>(property) ?? GetCustomAttribute<JsonConversionAttribute>(property.PropertyType);

                object? value;
                var item = arr[attribute.Index];

                if (converterAttribute != null)
                {
                    value = item.ToObject(property.PropertyType, new JsonSerializer
                    {
                        Converters = { (JsonConverter?)Activator.CreateInstance(converterAttribute.ConverterType) ?? throw new InvalidOperationException("Cannot create JsonConverter") }
                    });
                }
                else if (conversionAttribute != null)
                {
                    value = item.ToObject(property.PropertyType);
                }
                else
                {
                    value = item.ToObject(property.PropertyType, NostrSerializer.Serializer);
                }

                if (value != null && property.PropertyType.IsInstanceOfType(value))
                    property.SetValue(result, value);
                else
                {
                    if (value is JToken token)
                        if (token.Type == JTokenType.Null)
                            value = null;

                    if ((property.PropertyType == typeof(decimal)
                     || property.PropertyType == typeof(decimal?))
                     && (value != null && value.ToString()?.IndexOf("e", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        if (decimal.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var dec))
                            property.SetValue(result, dec);
                    }
                    else
                    {
                        property.SetValue(result, value == null ? null : Convert.ChangeType(value, property.PropertyType));
                    }
                }
            }

            if (arr.Count > maxIndex && result is IHaveAdditionalData resultWithData)
            {
                var unhandledData = arr.Skip(maxIndex)
                    .Select(x => x.ToObject<object>(NostrSerializer.Serializer))
                    .Where(x => x != null)
                    .ToArray();
                resultWithData.SetAdditionalData(unhandledData!);
            }
            
            if (arr.Count > maxIndex && result is IHaveAdditionalStringData resultWithStringData)
            {
                var unhandledData = arr.Skip(maxIndex)
                    .Select(x => x.Value<string?>())
                    .Where(x => x != null)
                    .ToArray();
                resultWithStringData.SetAdditionalData(unhandledData!);
            }

            return result;
        }

        private static bool IsSimple(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(type.GetGenericArguments()[0]);
            }
            return type.IsPrimitive
              || type.IsEnum
              || type == typeof(string)
              || type == typeof(decimal);
        }

        private static T? GetCustomAttribute<T>(MemberInfo memberInfo) where T : Attribute =>
            (T?)AttributeByMemberInfoAndTypeCache.GetOrAdd((memberInfo, typeof(T)), _ => memberInfo.GetCustomAttribute(typeof(T)));

        private static T? GetCustomAttribute<T>(Type type) where T : Attribute =>
            (T?)AttributeByTypeAndTypeCache.GetOrAdd((type, typeof(T)), _ => type.GetCustomAttribute(typeof(T)));
    }

    /// <summary>
    /// Mark property as an index in the array
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ArrayPropertyAttribute : Attribute
    {
        /// <summary>
        /// The index in the array
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="index"></param>
        public ArrayPropertyAttribute(int index)
        {
            Index = index;
        }
    }

    /// <summary>
    /// Used for conversion in ArrayConverter
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonConversionAttribute : Attribute
    {
    }
}
