using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Files;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Application.System.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.System
{
    public class SeedDataCommandTest : TestBase
    {
        private static readonly Constants Constants = new Constants
        {
            MinimumLevel = 1,
            MaximumLevel = 38,
            ExperienceForLevelCoefs = new[] { 1f, 1, 1 },
        };

        private static readonly ExperienceTable ExperienceTable = new ExperienceTable(Constants);
        private static readonly CharacterService CharacterService = new CharacterService(ExperienceTable, Constants);
        private static readonly ItemModifiers ItemModifiers = new FileItemModifiersSource().LoadItemModifiers();
        private static readonly ItemModifierService ItemModifierService = new ItemModifierService(ItemModifiers);

        [Test]
        public async Task ShouldInsertItemsFromItemSourceWithAllRanks()
        {
            var itemsSource = new Mock<IItemsSource>();
            itemsSource.Setup(s => s.LoadItems())
                .ReturnsAsync(new[]
                {
                    new ItemCreation { TemplateMbId = "a", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel() },
                    new ItemCreation { TemplateMbId = "b", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel() },
                });

            var seedDataCommandHandler = new SeedDataCommand.Handler(ActDb, itemsSource.Object, CreateAppEnv(),
                CharacterService, ExperienceTable, ItemModifierService);
            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);

            var items = await AssertDb.Items.ToArrayAsync();
            Assert.AreEqual(7 * 2, items.Length);

            var baseItem1 = items.First(i => i.TemplateMbId == "a" && i.Rank == 0);
            var baseItem2 = items.First(i => i.TemplateMbId == "b" && i.Rank == 0);
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
                .ReturnsAsync(new[] { new ItemCreation { TemplateMbId = "a", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel() } })
                .ReturnsAsync(Array.Empty<ItemCreation>());

            var seedDataCommandHandler = new SeedDataCommand.Handler(ActDb, itemsSource.Object, CreateAppEnv(),
                CharacterService, ExperienceTable, ItemModifierService);

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
                new Item { TemplateMbId = "a", Type = ItemType.HeadArmor, Rank = -3, Armor = new ItemArmorComponent { ArmArmor = 80 } },
                new Item { TemplateMbId = "a", Type = ItemType.HeadArmor, Rank = -2, Armor = new ItemArmorComponent { ArmArmor = 90 } },
                new Item { TemplateMbId = "a", Type = ItemType.HeadArmor, Rank = -1, Armor = new ItemArmorComponent { ArmArmor = 95 } },
                new Item { TemplateMbId = "a", Type = ItemType.HeadArmor, Rank = 0, Armor = new ItemArmorComponent { ArmArmor = 100 } },
                new Item { TemplateMbId = "a", Type = ItemType.HeadArmor, Rank = 1, Armor = new ItemArmorComponent { ArmArmor = 105 } },
                new Item { TemplateMbId = "a", Type = ItemType.HeadArmor, Rank = 2, Armor = new ItemArmorComponent { ArmArmor = 107 } },
                new Item { TemplateMbId = "a", Type = ItemType.HeadArmor, Rank = 3, Armor = new ItemArmorComponent { ArmArmor = 109 } },
            };
            ArrangeDb.Items.AddRange(oldItems);
            await ArrangeDb.SaveChangesAsync();

            var itemsSource = new Mock<IItemsSource>();
            itemsSource.Setup(s => s.LoadItems())
                .ReturnsAsync(new[] { new ItemCreation { TemplateMbId = "a", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel { ArmArmor = 1000 } } });

            var itemModifiers = new ItemModifiers
            {
                Armor = new[]
                {
                    new ArmorItemModifier { Name = "a", Armor = 0.4f },
                    new ArmorItemModifier { Name = "b", Armor = 0.6f },
                    new ArmorItemModifier { Name = "c", Armor = 0.8f },
                    new ArmorItemModifier { Name = "d", Armor = 1.2f },
                    new ArmorItemModifier { Name = "e", Armor = 1.4f },
                    new ArmorItemModifier { Name = "f", Armor = 1.6f },
                }
            };

            var itemModifierService = new ItemModifierService(itemModifiers);
            var seedDataCommandHandler = new SeedDataCommand.Handler(ActDb, itemsSource.Object, CreateAppEnv(),
                CharacterService, ExperienceTable, itemModifierService);
            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);
            var newItems = await AssertDb.Items.ToDictionaryAsync(i => i.Rank);
            Assert.AreEqual(7, newItems.Count, "Modifying an item added or removed one");
            Assert.AreEqual(400, newItems[-3].Armor!.ArmArmor);
            Assert.AreEqual(600, newItems[-2].Armor!.ArmArmor);
            Assert.AreEqual(800, newItems[-1].Armor!.ArmArmor);
            Assert.AreEqual(1000, newItems[0].Armor!.ArmArmor);
            Assert.AreEqual(1200, newItems[1].Armor!.ArmArmor);
            Assert.AreEqual(1400, newItems[2].Armor!.ArmArmor);
            Assert.AreEqual(1600, newItems[3].Armor!.ArmArmor);

            CollectionAssert.AreEquivalent(
                oldItems.Select(i => i.Id),
                newItems.Select(i => i.Value.Id),
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
