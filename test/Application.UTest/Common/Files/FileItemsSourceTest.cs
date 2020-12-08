using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crpg.Application.Common.Files;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Files
{
    public class FileItemsSourceTest
    {
        [Test]
        public async Task TestCanDeserializeFile()
        {
            var source = new FileItemsSource();
            var items = await source.LoadItems();
        }

        [Test]
        public async Task CheckNoDuplicatedMbId()
        {
            var duplicates = new List<string>();
            var mbIds = new HashSet<string>();

            var items = await new FileItemsSource().LoadItems();
            foreach (var item in items)
            {
                if (mbIds.Contains(item.TemplateMbId))
                {
                    duplicates.Add(item.TemplateMbId);
                }

                mbIds.Add(item.TemplateMbId);
            }

            if (duplicates.Count != 0)
            {
                Assert.Fail("Duplicate items: " + string.Join(", ", duplicates));
            }
        }

        [Test]
        public async Task CheckNoConflictingNameWithModifiedItems()
        {
            var errors = new List<string>();

            var items = (await new FileItemsSource().LoadItems()).ToArray();
            var itemsByName = new Dictionary<string, ItemCreation>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                if (itemsByName.TryGetValue(item.Name, out var conflictingItem))
                {
                    errors.Add($"Conflicting item name between {conflictingItem.TemplateMbId} and {item.TemplateMbId}");
                }

                itemsByName[item.Name] = item;
            }

            var itemModifiers = new FileItemModifiersSource().LoadItemModifiers();
            var itemModifier = new ItemModifierService(itemModifiers);
            foreach (var item in items)
            {
                foreach (int rank in new[] { -3, -2, -1, 1, 2, 3 })
                {
                    var modifiedItem = itemModifier.ModifyItem(item, rank);
                    if (itemsByName.TryGetValue(modifiedItem.Name, out var conflictingItem))
                    {
                        errors.Add($"Conflicting item name between {conflictingItem.TemplateMbId} and {item.TemplateMbId} rank {rank}");
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
            var errors = new List<string>();
            foreach (var item in items)
            {
                if (item.TemplateMbId.Contains("test") || item.TemplateMbId.Contains("dummy") || item.Name.Contains('_'))
                {
                    errors.Add(item.TemplateMbId);
                }
            }

            if (errors.Count != 0)
            {
                Assert.Fail($"Test items detected:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
            }
        }
    }
}
