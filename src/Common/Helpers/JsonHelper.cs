using System.Text.Json;

namespace Crpg.Common.Helpers
{
    public static class JsonHelper
    {
        public static T ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
        {
            var json = element.GetRawText();
            return JsonSerializer.Deserialize<T>(json, options)!;
        }
    }
}
