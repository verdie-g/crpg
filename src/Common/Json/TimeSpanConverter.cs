using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crpg.Common.Json
{
    /// <summary>
    /// Converts <see cref="TimeSpan"/> to <see cref="long"/> milliseconds.
    /// </summary>
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.FromMilliseconds(reader.GetInt64());
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((ulong)value.TotalMilliseconds);
        }
    }
}
