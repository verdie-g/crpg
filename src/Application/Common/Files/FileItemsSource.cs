using System.Text.Json;
using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Items.Models;
using Crpg.Common.Json;

namespace Crpg.Application.Common.Files;

internal class FileItemsSource : IItemsSource
{
    private static readonly string ItemsPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "/Common/Files/items.json";

    public async Task<IEnumerable<ItemCreation>> LoadItems()
    {
        await using var file = File.OpenRead(ItemsPath);
        return (await JsonSerializer.DeserializeAsync<IEnumerable<ItemCreation>>(file, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonArrayStringEnumFlagsConverterFactory(), new JsonStringEnumConverter() },
        }).AsTask())!;
    }
}
