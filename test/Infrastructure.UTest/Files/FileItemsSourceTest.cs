﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crpg.Application.Common.Services;
using Crpg.Infrastructure.Files;
using NUnit.Framework;

namespace Crpg.Infrastructure.UTest.Files
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
                if (mbIds.Contains(item.MbId))
                {
                    duplicates.Add(item.MbId);
                }

                mbIds.Add(item.MbId);
            }

            if (duplicates.Count != 0)
            {
                Assert.Fail("Duplicate items: " + string.Join(", ", duplicates));
            }
        }

        [Test]
        public async Task CheckNoConflictingNameWithModifiedItems()
        {
            var itemsByMbId = (await new FileItemsSource().LoadItems()).ToDictionary(i => i.MbId);
            var itemModifier = new ItemModifierService();
            foreach (var item in itemsByMbId.Values)
            {
                foreach (int rank in new[] { -3, -2, -1, 1, 2, 3 })
                {
                    var modifiedItem = itemModifier.ModifyItem(item, rank);
                    if (itemsByMbId.TryGetValue(modifiedItem.MbId, out var conflictingItem))
                    {
                        Assert.Fail("Conflicting item name between {0} and {1} rank {2}", conflictingItem.MbId, item.MbId, rank);
                    }
                }
            }
        }
    }
}
