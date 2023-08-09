using System.Xml.Linq;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Strategus;
using Crpg.Module.Helpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Crpg.Module.DataExport;

internal class SettlementExporter : IDataExporter
{
    private const string SettlementsFile = "../../Modules/SandBox/ModuleData/settlements.xml";


    public Task Export(string gitRepoPath)
    {
        List<CrpgSettlementCreation> settlements = new();

        var settlementsDoc = XDocument.Load(SettlementsFile);
        foreach (var settlementNode in settlementsDoc.Descendants("Settlement"))
        {
            if (settlementNode.Attribute("type")?.Value == "Hideout")
            {
                continue;
            }

            CrpgSettlementCreation settlement = new()
            {
                Name = settlementNode.Attribute("name")!.Value.Split('}')[1],
                Type = GetSettlementType(settlementNode),
                Culture = ParseCulture(settlementNode.Attribute("culture")!.Value),
                Position = new Point
                {
                    Coordinates = new[]
                    {
                        double.Parse(settlementNode.Attribute("posX")!.Value),
                        double.Parse(settlementNode.Attribute("posY")!.Value),
                    },
                },
                Scene = GetSettlementScene(settlementNode),
            };

            settlements.Add(settlement);
        }

        var serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new JsonConverter[] { new ArrayStringEnumFlagsConverter(), new StringEnumConverter() },
        });

        using StreamWriter s = new(Path.Combine(gitRepoPath, "data", "settlements.json"));
        serializer.Serialize(s, settlements);
        return Task.CompletedTask;
    }

    public Task ImageExport(string gitRepoPath)
    {
        List<CrpgSettlementCreation> settlements = new();

        var settlementsDoc = XDocument.Load(SettlementsFile);
        foreach (var settlementNode in settlementsDoc.Descendants("Settlement"))
        {
            if (settlementNode.Attribute("type")?.Value == "Hideout")
            {
                continue;
            }

            CrpgSettlementCreation settlement = new()
            {
                Name = settlementNode.Attribute("name")!.Value.Split('}')[1],
                Type = GetSettlementType(settlementNode),
                Culture = ParseCulture(settlementNode.Attribute("culture")!.Value),
                Position = new Point
                {
                    Coordinates = new[]
                    {
                        double.Parse(settlementNode.Attribute("posX")!.Value),
                        double.Parse(settlementNode.Attribute("posY")!.Value),
                    },
                },
                Scene = GetSettlementScene(settlementNode),
            };

            settlements.Add(settlement);
        }

        var serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new JsonConverter[] { new ArrayStringEnumFlagsConverter(), new StringEnumConverter() },
        });

        using StreamWriter s = new(Path.Combine(gitRepoPath, "data", "settlements.json"));
        serializer.Serialize(s, settlements);
        return Task.CompletedTask;
    }

    public Task ComputeAutoStats(string gitRepoPath)
    {
        return Task.CompletedTask;
    }

    public Task RefundArmor(string gitRepoPath)
    {
        return Task.CompletedTask;
    }

    public Task RefundCrossbow(string gitRepoPath)
    {
        return Task.CompletedTask;
    }

    public Task RefundBow(string gitRepoPath)
    {
        return Task.CompletedTask;
    }

    public Task RefundThrowing(string gitRepoPath)
    {
        return Task.CompletedTask;
    }

    public Task RefundCav(string gitRepoPath)
    {
        return Task.CompletedTask;
    }

    public Task RefundShield(string gitRepoPath)
    {
        return Task.CompletedTask;
    }

    public Task Scale(string gitRepoPath)
    {
        return Task.CompletedTask;
    }
    private static CrpgCulture ParseCulture(string mbCulture) =>
        (CrpgCulture)Enum.Parse(typeof(CrpgCulture), mbCulture.Split('.')[1], true);

    private static CrpgSettlementType GetSettlementType(XElement settlementNode)
    {
        var componentNode = settlementNode.Element("Components")!.Elements().First();
        if (componentNode.Name.LocalName == "Village")
        {
            return CrpgSettlementType.Village;
        }

        var isCastleAttribute = componentNode.Attribute("is_castle");
        return isCastleAttribute != null && bool.Parse(isCastleAttribute.Value)
            ? CrpgSettlementType.Castle
            : CrpgSettlementType.Town;
    }

    private static string GetSettlementScene(XElement settlementNode)
    {
        var locationNode = settlementNode.Descendants("Location").First(e => e.Attribute("id")!.Value.Contains("center"));
        return locationNode.Attribute("scene_name")!.Value;
    }
}
