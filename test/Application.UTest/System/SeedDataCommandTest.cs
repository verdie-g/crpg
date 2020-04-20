using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Games.Commands;
using Crpg.Application.System.Commands;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.System
{
    public class SeedDataCommandTest : TestBase
    {
        [Test]
        public async Task CheckSeedContainsAllStarterItems()
        {
            await new SeedDataCommand.Handler(Db).Handle(new SeedDataCommand(), CancellationToken.None);
            foreach (var set in UpsertGameUserCommand.Handler.DefaultItemsSets)
            {
                AssertItemExists(set.HeadItemMbId);
                AssertItemExists(set.CapeItemMbId);
                AssertItemExists(set.BodyItemMbId);
                AssertItemExists(set.HandItemMbId);
                AssertItemExists(set.LegItemMbId);
                AssertItemExists(set.HorseHarnessItemMbId);
                AssertItemExists(set.HorseItemMbId);
                AssertItemExists(set.Weapon1ItemMbId);
                AssertItemExists(set.Weapon2ItemMbId);
                AssertItemExists(set.Weapon3ItemMbId);
                AssertItemExists(set.Weapon4ItemMbId);
            }
        }

        [Test]
        public async Task CheckSeedDoestContainDuplicateItems()
        {
            await new SeedDataCommand.Handler(Db).Handle(new SeedDataCommand(), CancellationToken.None);

            var itemMbIds = new HashSet<string>();
            foreach (var item in await Db.Items.ToArrayAsync())
            {
                if (itemMbIds.Contains(item.MbId))
                {
                    Assert.Fail($"Duplicate item \"{item.MbId}\"");
                }

                itemMbIds.Add(item.MbId);
            }
        }

        private void AssertItemExists(string? mbId)
        {
            if (mbId == null)
            {
                return;
            }

            var item = Db.Items.FirstOrDefault(i => i.MbId == mbId);
            if (item == null)
            {
                Assert.Fail($"Item \"{mbId}\" doesn't exist");
            }
        }
    }
}