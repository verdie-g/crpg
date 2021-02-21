using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crpg.Common.Json
{
    public class JsonArrayStringEnumFlagsConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsEnum && typeToConvert.IsDefined(typeof(FlagsAttribute), false);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(JsonArrayStringEnumFlagsConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    /// <summary>
    /// Converts enum with <see cref="System.FlagsAttribute"/> to a JSON array of strings.
    /// </summary>
    public class JsonArrayStringEnumFlagsConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        private static readonly TypeCode EnumTypeCode = Type.GetTypeCode(typeof(T));
        private static readonly Dictionary<string, long> EnumValues =
            Enum.GetValues(typeof(T))
                .Cast<T>()
                .Distinct() // In case "enum { A, B = A }", A.A or A.B are written as "A".
                .ToDictionary(e => e.ToString(), e => Convert.ToInt64(e));

        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsEnum && typeToConvert.IsDefined(typeof(FlagsAttribute), false);

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected JSON array for enum flags type");
            }

            long flags = 0;
            while (reader.Read() && reader.TokenType == JsonTokenType.String)
            {
                if (EnumValues.TryGetValue(reader.GetString()!, out long flagVal))
                {
                    flags |= flagVal;
                }
            }

            if (reader.TokenType != JsonTokenType.EndArray)
            {
                throw new JsonException("Expected JSON array end");
            }

            return (T)Convert.ChangeType(flags, EnumTypeCode);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            long valueInt = Convert.ToInt64(value);

            writer.WriteStartArray();
            foreach (var flag in EnumValues)
            {
                if ((valueInt & flag.Value) != 0)
                {
                    writer.WriteStringValue(flag.Key);
                }
            }

            writer.WriteEndArray();
        }
    }
}
