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
        Assert.DoesNotThrowAsync(() => source.LoadItems());
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

        if (duplicates.Count != 0)
        {
            Assert.Fail("Duplicate items: " + string.Join(", ", duplicates));
        }
    }

    [Test]
    public async Task CheckNoConflictingNameWithModifiedItems()
    {
        List<string> errors = new();

        var items = (await new FileItemsSource().LoadItems()).ToArray();
        Dictionary<string, ItemCreation> itemsByName = new(StringComparer.Ordinal);
        foreach (var item in items)
        {
            if (itemsByName.TryGetValue(item.Name, out var conflictingItem))
            {
                errors.Add($"Conflicting item name between {conflictingItem.Id} and {item.Id}");
            }

            itemsByName[item.Name] = item;
        }

        var itemModifiers = new FileItemModifiersSource().LoadItemModifiers();
        ItemModifierService itemModifier = new(itemModifiers);
        foreach (var item in items)
        {
            foreach (int rank in new[] { -3, -2, -1, 1, 2, 3 })
            {
                var modifiedItem = itemModifier.ModifyItem(new Item
                {
                    Name = item.Name,
                    Type = item.Type,
                    Armor = new ItemArmorComponent(),
                    Mount = new ItemMountComponent(),
                    PrimaryWeapon = new ItemWeaponComponent(),
                }, rank);
                if (itemsByName.TryGetValue(modifiedItem.Name, out var conflictingItem))
                {
                    errors.Add($"Conflicting item name between {conflictingItem.Id} and {item.Id} rank {rank}");
                }
            }
        }

        if (errors.Count != 0)
        {
            Assert.Fail("- " + string.Join($"{Environment.NewLine}- ", errors));
        }
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

        if (errors.Count != 0)
        {
            Assert.Fail($"Test items detected:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
        }
    }

    [Test]
    public async Task CheckPositivePrices()
    {
        var items = await new FileItemsSource().LoadItems();
        List<string> errors = new();
        foreach (var item in items)
        {
            if (item.Price <= 0)
            {
                errors.Add(item.Id);
            }
        }

        if (errors.Count != 0)
        {
            Assert.Fail($"Items with zero or negative price:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
        }
    }
}
