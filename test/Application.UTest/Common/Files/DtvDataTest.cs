using System.Xml.Linq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Files;

public class DtvDataTest
{
    [Test]
    public void CheckBotExists()
    {
        string classDivisionsXmlPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "/ModuleData/class_divisions.xml";
        XDocument classDivisionsDoc = XDocument.Load(classDivisionsXmlPath);
        var classDivisionIds = classDivisionsDoc
            .Descendants("MPClassDivision")
            .Select(el => el.Attribute("id")!.Value)
            .ToHashSet();

        string dtvDataXmlPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "/ModuleData/dtv_data.xml";
        XDocument dtvDataDoc = XDocument.Load(dtvDataXmlPath);
        string[] classDivisionIdsFromDtv = dtvDataDoc
            .Descendants("Group")
            .Select(el => el.Attribute("classDivisionId")!.Value)
            .ToArray();

        Assert.Multiple(() =>
        {
            foreach (string id in classDivisionIdsFromDtv)
            {
                if (!classDivisionIds.Contains(id))
                {
                    string closestClassDivisionId = TestHelper.FindClosestString(id, classDivisionIds);
                    Assert.Fail($"Class division {id} was not found in class_divisions.xml. Did you mean {closestClassDivisionId}?");
                }
            }
        });
    }
}
