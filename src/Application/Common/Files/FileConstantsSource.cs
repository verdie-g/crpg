using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crpg.Application.Common.Files;

internal class FileConstantsSource
{
    private static readonly string ConstantsPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "/Common/Files/constants.json";

    public Constants LoadConstants()
    {
        string fileContent = File.ReadAllText(ConstantsPath);
        return JsonSerializer.Deserialize<Constants>(fileContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new JsonStringEnumConverter() },
        })!;
    }
}
