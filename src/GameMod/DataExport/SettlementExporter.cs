using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Crpg.GameMod.Api.Models;
using Crpg.GameMod.Api.Models.Strategus;
using Crpg.GameMod.Helpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Crpg.GameMod.DataExport
{
    internal class SettlementExporter : IDataExporter
    {
        private const string SettlementsFile = "../../Modules/SandBox/ModuleData/settlements.xml";

        public Task Export(string outputPath)
        {
            var settlements = new List<CrpgStrategusSettlementCreation>();

            var settlementsDoc = XDocument.Load(SettlementsFile);
            foreach (var settlementNode in settlementsDoc.Descendants("Settlement"))
            {
                if (settlementNode.Attribute("type")?.Value == "Hideout")
                {
                    continue;
                }

                var settlement = new CrpgStrategusSettlementCreation
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

            using var s = new StreamWriter(Path.Combine(outputPath, "settlements.json"));
            serializer.Serialize(s, settlements);
            return Task.CompletedTask;
        }

        private static CrpgCulture ParseCulture(string mbCulture) =>
            (CrpgCulture)Enum.Parse(typeof(CrpgCulture), mbCulture.Split('.')[1], true);

        private static CrpgStrategusSettlementType GetSettlementType(XElement settlementNode)
        {
            var componentNode = settlementNode.Element("Components")!.Elements().First();
            if (componentNode.Name.LocalName == "Village")
            {
                return CrpgStrategusSettlementType.Village;
            }

            var isCastleAttribute = componentNode.Attribute("is_castle");
            return isCastleAttribute != null && bool.Parse(isCastleAttribute.Value)
                ? CrpgStrategusSettlementType.Castle
                : CrpgStrategusSettlementType.Town;
        }

        private static string GetSettlementScene(XElement settlementNode)
        {
            var locationNode = settlementNode.Descendants("Location").First(e => e.Attribute("id")!.Value.Contains("center"));
            return locationNode.Attribute("scene_name")!.Value;
        }
    }
}
