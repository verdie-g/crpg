using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Crpg.GameMod.Helpers.Json
{
    /// <summary>
    /// Converts enum with <see cref="System.FlagsAttribute"/> to a JSON array of strings.
    /// </summary>
    internal class ArrayStringEnumFlagsConverter : JsonConverter
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, long>> EnumValuesByType = new();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            long valueInt = Convert.ToInt64(value);

            writer.WriteStartArray();
            foreach (var flag in GetEnumValues(value.GetType()))
            {
                if ((valueInt & flag.Value) != 0)
                {
                    writer.WriteValue(flag.Key);
                }
            }

            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new JsonException("Expected JSON array for enum flags type");
            }

            var enumValues = GetEnumValues(objectType);
            long flags = 0;
            while (reader.Read() && reader.TokenType == JsonToken.String)
            {
                if (enumValues.TryGetValue(reader.Value.ToString(), out long flagVal))
                {
                    flags |= flagVal;
                }
            }

            if (reader.TokenType != JsonToken.EndArray)
            {
                throw new JsonException("Expected JSON array end");
            }

            return flags;
        }

        public override bool CanConvert(Type objectType) =>
            objectType.IsEnum && objectType.IsDefined(typeof(FlagsAttribute), false);

        private static Dictionary<string, long> GetEnumValues(Type enumType)
        {
            if (EnumValuesByType.TryGetValue(enumType, out var enumValues))
            {
                return enumValues;
            }

            enumValues = new Dictionary<string, long>();
            foreach (object val in Enum.GetValues(enumType))
            {
                enumValues[val.ToString()] = (long)val;
            }

            EnumValuesByType[enumType] = enumValues;
            return enumValues;
        }
    }
}
