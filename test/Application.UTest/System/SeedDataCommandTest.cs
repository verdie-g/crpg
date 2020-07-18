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

            var seedDataCommandHandler = new SeedDataCommand.Handler(Db, itemsSource.Object, CreateAppEnv(), new ItemModifierService());
            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);

            var items = await Db.Items.ToArrayAsync();
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

            var seedDataCommandHandler = new SeedDataCommand.Handler(Db, itemsSource.Object, CreateAppEnv(), new ItemModifierService());

            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);
            var items = await Db.Items.ToArrayAsync();
            Assert.AreEqual(7, items.Length);

            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);
            items = await Db.Items.ToArrayAsync();
            Assert.AreEqual(0, items.Length);
        }

        [Test]
        public async Task ModifiedItemInSourceShouldBeModifiedInDatabase()
        {
            var itemsSource = new Mock<IItemsSource>();
            itemsSource.SetupSequence(s => s.LoadItems())
                .ReturnsAsync(new[] { new ItemCreation { MbId = "a", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel { ArmArmor = 100 } } })
                .ReturnsAsync(new[] { new ItemCreation { MbId = "a", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel { ArmArmor = 1000 } } });

            var seedDataCommandHandler = new SeedDataCommand.Handler(Db, itemsSource.Object, CreateAppEnv(), new ItemModifierService());

            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);
            var oldItems = await Db.Items.ToArrayAsync();
            Assert.AreEqual(7, oldItems.Length);
            foreach (var item in oldItems)
            {
                Assert.Greater(item.Armor!.ArmArmor, 50);
                Assert.Less(item.Armor!.ArmArmor, 150);
            }

            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);
            var newItems = await Db.Items.ToArrayAsync();
            Assert.AreEqual(7, newItems.Length);
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
