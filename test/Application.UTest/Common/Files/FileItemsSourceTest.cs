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

        Assert.That(errors, Is.Empty,
            "- " + string.Join($"{Environment.NewLine}- ", errors));
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
}
