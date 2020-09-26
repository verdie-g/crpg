using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Application.System.Commands;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.System
{
    public class SeedDataCommandTest : TestBase
    {
        [Test]
        public async Task ShouldInsertItemsFromItemSourceWithAllRanks()
        {
            var itemsSource = new Mock<IItemsSource>();
            itemsSource.Setup(s => s.LoadItems())
                .ReturnsAsync(new[]
                {
                    new ItemCreation { MbId = "a", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel() },
                    new ItemCreation { MbId = "b", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel() },
                });

            var seedDataCommandHandler = new SeedDataCommand.Handler(ActDb, itemsSource.Object, CreateAppEnv(), new ItemModifierService());
            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);

            var items = await AssertDb.Items.ToArrayAsync();
            Assert.AreEqual(7 * 2, items.Length);

            var baseItem1 = items.First(i => i.MbId == "a" && i.Rank == 0);
            var baseItem2 = items.First(i => i.MbId == "b" && i.Rank == 0);
            foreach (var baseItem in new[] { baseItem1, baseItem2 })
            {
                foreach (int rank in new[] { -3, -2, -1, 1, 2, 3 })
                {
                    Assert.NotNull(items.FirstOrDefault(i => i.BaseItemId == baseItem.Id && i.Rank == rank));
                }
            }
        }

        [Test]
        public async Task DeletedItemInSourceShouldBeDeletedInDatabase()
        {
            var itemsSource = new Mock<IItemsSource>();
            itemsSource.SetupSequence(s => s.LoadItems())
                .ReturnsAsync(new[] { new ItemCreation { MbId = "a", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel() } })
                .ReturnsAsync(Array.Empty<ItemCreation>());

            var seedDataCommandHandler = new SeedDataCommand.Handler(ActDb, itemsSource.Object, CreateAppEnv(), new ItemModifierService());

            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);
            var items = await ArrangeDb.Items.ToArrayAsync();
            Assert.AreEqual(7, items.Length);

            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);
            items = await ArrangeDb.Items.ToArrayAsync();
            Assert.AreEqual(0, items.Length);
        }

        [Test]
        public async Task ModifiedItemInSourceShouldBeModifiedInDatabase()
        {
            var oldItems = new[]
            {
                new Item { MbId = "damaged_a", Type = ItemType.HeadArmor, Rank = -3, Armor = new ItemArmorComponent { ArmArmor = 80 } },
                new Item { MbId = "battered_a", Type = ItemType.HeadArmor, Rank = -2, Armor = new ItemArmorComponent { ArmArmor = 90 } },
                new Item { MbId = "rusty_a", Type = ItemType.HeadArmor, Rank = -1, Armor = new ItemArmorComponent { ArmArmor = 95 } },
                new Item { MbId = "a", Type = ItemType.HeadArmor, Rank = 0, Armor = new ItemArmorComponent { ArmArmor = 100 } },
                new Item { MbId = "thick_a", Type = ItemType.HeadArmor, Rank = 1, Armor = new ItemArmorComponent { ArmArmor = 105 } },
                new Item { MbId = "reinforced_a", Type = ItemType.HeadArmor, Rank = 2, Armor = new ItemArmorComponent { ArmArmor = 107 } },
                new Item { MbId = "lordly_a", Type = ItemType.HeadArmor, Rank = 3, Armor = new ItemArmorComponent { ArmArmor = 109 } },
            };
            ArrangeDb.Items.AddRange(oldItems);
            await ArrangeDb.SaveChangesAsync();

            var itemsSource = new Mock<IItemsSource>();
            itemsSource.Setup(s => s.LoadItems())
                .ReturnsAsync(new[] { new ItemCreation { MbId = "a", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel { ArmArmor = 1000 } } });

            var seedDataCommandHandler = new SeedDataCommand.Handler(ActDb, itemsSource.Object, CreateAppEnv(), new ItemModifierService());
            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);
            var newItems = await AssertDb.Items.ToArrayAsync();
            Assert.AreEqual(7, newItems.Length, "Modifying an item added or removed one");
            foreach (var item in newItems)
            {
                Assert.Greater(item.Armor!.ArmArmor, 500);
                Assert.Less(item.Armor!.ArmArmor, 1500);
            }

            CollectionAssert.AreEquivalent(
                oldItems.Select(i => i.Id),
                newItems.Select(i => i.Id),
                "Modified items were recreated instead of just modified");
        }

        private IApplicationEnvironment CreateAppEnv()
        {
            var appEnv = new Mock<IApplicationEnvironment>();
            appEnv.Setup(e => e.Environment).Returns(HostingEnvironment.Production);
            return appEnv.Object;
        }
    }
}
