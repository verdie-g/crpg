using System.Text.Json;
using Crpg.Application.Common.Services;

namespace Crpg.Application.Common.Files;

internal class FileItemModifiersSource
{
    private static readonly string ItemModifiersPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "/Common/Files/item-modifiers.json";

    public ItemModifiers LoadItemModifiers()
    {
        string fileContent = File.ReadAllText(ItemModifiersPath);
        return JsonSerializer.Deserialize<ItemModifiers>(fileContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
        })!;
    }
}
