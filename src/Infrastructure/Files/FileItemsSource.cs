using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Items.Models;

namespace Crpg.Infrastructure.Files
{
    public class FileItemsSource : IItemsSource
    {
        private static readonly string ItemsPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "/Files/items.json";

        public async Task<IEnumerable<ItemCreation>> LoadItems()
        {
            await using var file = File.OpenRead(ItemsPath);
            return await JsonSerializer.DeserializeAsync<IEnumerable<ItemCreation>>(file, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() },
            }).AsTask();
        }
    }
}