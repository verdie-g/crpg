using System.Xml.Linq;
using Crpg.Application.Common.Files;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Items;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Files;

public class FileItemsSourceTest
{
    [Test]
    public void TestCanDeserializeFile()
    {
        FileItemsSource source = new();
        Assert.That(() => source.LoadItems(), Throws.Nothing);
    }

    [Test]
    public async Task CheckNoDuplicatedId()
    {
        List<string> duplicates = new();
        HashSet<string> ids = new();

        var items = await new FileItemsSource().LoadItems();
        foreach (var item in items)
        {
            if (ids.Contains(item.Id))
            {
                duplicates.Add(item.Id);
            }

            ids.Add(item.Id);
        }

        Assert.That(duplicates, Is.Empty,
           "Duplicate items: " + string.Join(", ", duplicates));
    }

    [Test]
    public async Task CheckNoTestItems()
    {
        var items = await new FileItemsSource().LoadItems();
        List<string> errors = new();
        foreach (var item in items)
        {
            if (item.Id.Contains("test") || item.Id.Contains("dummy") || item.Name.Contains('_'))
            {
                errors.Add(item.Id);
            }
        }

        Assert.That(errors, Is.Empty,
            $"Test items detected:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
    }

    [Test]
    public async Task CheckItemTier()
    {
        var items = await new FileItemsSource().LoadItems();
        List<string> errors = new();
        foreach (var item in items)
        {
            if (item.Tier > 13.1)
            {
                errors.Add(item.Id);
            }
        }

        Assert.That(errors, Is.Empty,
            $"Item with too higher tier:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
    }

    [Test]
    public async Task CheckPriceRange()
    {
        var items = await new FileItemsSource().LoadItems();
        List<string> errors = new();
        foreach (var item in items)
        {
            if (item.Price <= 0 || item.Price > 100_000)
            {
                errors.Add(item.Id);
            }
        }

        Assert.That(errors, Is.Empty,
            $"Items with zero, or negative price or price too high:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
    }

    [Test]
    public async Task CheckBotItemsExist()
    {
        var items = (await new FileItemsSource().LoadItems())
            .Select(i => i.Id)
            .ToHashSet();

        string charactersXmlPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                                   + "/ModuleData/characters.xml";
        XDocument charactersDoc = XDocument.Load(charactersXmlPath);
        string[] itemIdsFromXml = charactersDoc
            .Descendants("equipment")
            .Select(el => el.Attribute("id")!.Value["Item.".Length..])
            .ToArray();

        Assert.Multiple(() =>
        {
            foreach (string itemId in itemIdsFromXml)
            {
                if (!items.Contains(itemId))
                {
                    string closestItemId = TestHelper.FindClosestString(itemId, items);
                    Assert.Fail($"Character item {itemId} was not found in items.json. Did you mean {closestItemId}?");
                }
            }
        });
    }
}
