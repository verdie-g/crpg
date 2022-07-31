using Newtonsoft.Json;

namespace Crpg.Module.Helpers.Json;

/// <summary>
/// Converts <see cref="TimeSpan"/> to <see cref="long"/> milliseconds.
/// </summary>
internal class TimeSpanConverter : JsonConverter<TimeSpan>
{
    public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
    {
        writer.WriteValue((ulong)value.TotalMilliseconds);
    }

    public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.Integer)
        {
            throw new JsonException("Expected integer for timespan type");
        }

        return TimeSpan.FromMilliseconds((ulong)reader.Value);
    }
}
