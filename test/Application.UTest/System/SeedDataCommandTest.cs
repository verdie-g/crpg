using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Files;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Application.Settlements.Models;
using Crpg.Application.System.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.System
{
    public class SeedDataCommandTest : TestBase
    {
        private static readonly Region[] Regions = Enum.GetValues(typeof(Region)).Cast<Region>().ToArray();
        private static readonly IExperienceTable ExperienceTable = Mock.Of<IExperienceTable>();
        private static readonly ICharacterService CharacterService = Mock.Of<ICharacterService>();
        private static readonly IStrategusMap StrategusMap = Mock.Of<IStrategusMap>();
        private static readonly ItemModifiers ItemModifiers = new FileItemModifiersSource().LoadItemModifiers();
        private static readonly ItemModifierService ItemModifierService = new(ItemModifiers);
        private static readonly ItemValueModel ItemValueModel = new();

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
                CharacterService, ExperienceTable, StrategusMap, Mock.Of<ISettlementsSource>(),
                ItemValueModel, ItemModifierService);
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
        public async Task DeletedItemInSourceShouldBeDeletedInDatabaseAndUserReimbursed()
        {
            var itemsSource = new Mock<IItemsSource>();
            itemsSource.SetupSequence(s => s.LoadItems())
                .ReturnsAsync(new[] { new ItemCreation { TemplateMbId = "a", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel() } })
                .ReturnsAsync(Array.Empty<ItemCreation>());

            var seedDataCommandHandler = new SeedDataCommand.Handler(ActDb, itemsSource.Object, CreateAppEnv(),
                CharacterService, ExperienceTable, StrategusMap, Mock.Of<ISettlementsSource>(),
                ItemValueModel, ItemModifierService);
            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);
            var items = await AssertDb.Items.ToArrayAsync();
            Assert.AreEqual(7, items.Length);

            // Users buy the new item and equip it.
            var user0 = new User { Gold = 100, HeirloomPoints = 0 };
            var ownedItemRank0ForUser0 = new OwnedItem { User = user0, ItemId = items.First(i => i.Rank == 0).Id };
            var character0 = new Character
            {
                User = user0,
                EquippedItems =
                {
                    new EquippedItem { OwnedItem = ownedItemRank0ForUser0, Slot = ItemSlot.Weapon0 },
                    new EquippedItem { OwnedItem = ownedItemRank0ForUser0, Slot = ItemSlot.Weapon1 },
                },
            };

            var user1 = new User { Gold = 200, HeirloomPoints = 0 };
            var ownedItemRank0ForUser1 = new OwnedItem { User = user1, ItemId = items.First(i => i.Rank == 0).Id };
            var ownedItemRank1ForUser1 = new OwnedItem { User = user1, ItemId = items.First(i => i.Rank == 1).Id };
            var character1 = new Character
            {
                User = user1,
                EquippedItems = { new EquippedItem { OwnedItem = ownedItemRank0ForUser1, Slot = ItemSlot.Weapon0 } },
            };

            var character2 = new Character
            {
                User = user1,
                EquippedItems = { new EquippedItem { OwnedItem = ownedItemRank1ForUser1, Slot = ItemSlot.Weapon1 } },
            };

            ArrangeDb.Characters.AddRange(character0, character1, character2);
            await ArrangeDb.SaveChangesAsync();

            await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);
            items = await AssertDb.Items.ToArrayAsync();
            Assert.AreEqual(0, items.Length);
            var users = await AssertDb.Users.ToArrayAsync();
            Assert.Greater(users[0].Gold, 100);
            Assert.AreEqual(0, users[0].HeirloomPoints);
            Assert.Greater(users[1].Gold, 200);
            Assert.AreEqual(1, users[1].HeirloomPoints);
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
                },
            };

            var itemModifierService = new ItemModifierService(itemModifiers);
            var seedDataCommandHandler = new SeedDataCommand.Handler(ActDb, itemsSource.Object, CreateAppEnv(),
                CharacterService, ExperienceTable, StrategusMap, Mock.Of<ISettlementsSource>(),
                ItemValueModel, itemModifierService);
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

        [Test]
        public async Task ShouldAddSettlementIfDoesntExistsInDb()
        {
            var settlementsSource = new Mock<ISettlementsSource>();
            settlementsSource.Setup(s => s.LoadStrategusSettlements())
                .ReturnsAsync(new[]
                {
                    new SettlementCreation { Name = "a", Position = new Point(0, 0) },
                    new SettlementCreation { Name = "b", Position = new Point(0, 0) },
                });

            var handler = new SeedDataCommand.Handler(ActDb, Mock.Of<IItemsSource>(), CreateAppEnv(), CharacterService,
                ExperienceTable, StrategusMap, settlementsSource.Object, ItemValueModel,
                ItemModifierService);
            await handler.Handle(new SeedDataCommand(), CancellationToken.None);

            var settlements = await AssertDb.Settlements.ToArrayAsync();
            Assert.AreEqual(2 * Regions.Length, settlements.Length);

            Assert.NotZero(settlements[0].Id);
            Assert.AreEqual("a", settlements[0].Name);
            Assert.AreEqual(Region.Europe, settlements[0].Region);

            Assert.NotZero(settlements[1].Id);
            Assert.AreEqual("a", settlements[1].Name);
            Assert.AreEqual(Region.NorthAmerica, settlements[1].Region);

            Assert.NotZero(settlements[2].Id);
            Assert.AreEqual("a", settlements[2].Name);
            Assert.AreEqual(Region.Asia, settlements[2].Region);

            Assert.AreEqual("b", settlements[3].Name);
        }

        [Test]
        public async Task ShouldModifySettlementIfAlreadyExistsInDb()
        {
            var dbSettlements = new[]
            {
                new Settlement
                {
                    Name = "a",
                    Type = SettlementType.Castle,
                    Culture = Culture.Aserai,
                    Region = Region.Europe,
                    Position = new Point(1, 2),
                    Scene = "abc",
                },
                new Settlement
                {
                    Name = "a",
                    Type = SettlementType.Castle,
                    Culture = Culture.Aserai,
                    Region = Region.NorthAmerica,
                    Position = new Point(1, 2),
                    Scene = "abc",
                },
                new Settlement
                {
                    Name = "a",
                    Type = SettlementType.Castle,
                    Culture = Culture.Aserai,
                    Region = Region.Asia,
                    Position = new Point(1, 2),
                    Scene = "abc",
                },
            };
            ArrangeDb.Settlements.AddRange(dbSettlements);
            await ArrangeDb.SaveChangesAsync();

            var settlementsSource = new Mock<ISettlementsSource>();
            settlementsSource.Setup(s => s.LoadStrategusSettlements())
                .ReturnsAsync(new[]
                {
                    new SettlementCreation
                    {
                        Name = "a",
                        Type = SettlementType.Town,
                        Culture = Culture.Battania,
                        Position = new Point(3, 4),
                        Scene = "def",
                    },
                });

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.TranslatePositionForRegion(It.IsAny<Point>(), Region.Europe, Region.Europe))
                .Returns(new Point(3, 4));
            strategusMapMock
                .Setup(m => m.TranslatePositionForRegion(It.IsAny<Point>(), Region.Europe, Region.NorthAmerica))
                .Returns(new Point(4, 5));
            strategusMapMock
                .Setup(m => m.TranslatePositionForRegion(It.IsAny<Point>(), Region.Europe, Region.Asia))
                .Returns(new Point(5, 6));

            var handler = new SeedDataCommand.Handler(ActDb, Mock.Of<IItemsSource>(), CreateAppEnv(), CharacterService,
                ExperienceTable, strategusMapMock.Object, settlementsSource.Object, ItemValueModel, ItemModifierService);
            await handler.Handle(new SeedDataCommand(), CancellationToken.None);

            var settlements = await AssertDb.Settlements.ToArrayAsync();
            Assert.AreEqual(Regions.Length, settlements.Length);

            Assert.AreEqual(Region.Europe, settlements[0].Region);
            Assert.AreEqual(new Point(3, 4), settlements[0].Position);
            Assert.AreEqual(Region.NorthAmerica, settlements[1].Region);
            Assert.AreEqual(new Point(4, 5), settlements[1].Position);
            Assert.AreEqual(Region.Asia, settlements[2].Region);
            Assert.AreEqual(new Point(5, 6), settlements[2].Position);
            for (int i = 0; i < settlements.Length; i += 1)
            {
                Assert.AreEqual(dbSettlements[i].Id, settlements[i].Id);
                Assert.AreEqual("a", settlements[i].Name);
                Assert.AreEqual(SettlementType.Town, settlements[i].Type);
                Assert.AreEqual(Culture.Battania, settlements[i].Culture);
                Assert.AreEqual("def", settlements[i].Scene);
            }
        }

        [Test]
        public async Task ShouldDeleteSettlementIfDoesntExistInSourceAnymore()
        {
            var dbSettlements = Regions
                .Select(r => new Settlement { Name = "c", Region = r })
                .ToArray();
            ArrangeDb.Settlements.AddRange(dbSettlements);
            await ArrangeDb.SaveChangesAsync();

            var settlementsSource = new Mock<ISettlementsSource>();
            settlementsSource.Setup(s => s.LoadStrategusSettlements())
                .ReturnsAsync(Array.Empty<SettlementCreation>());

            var handler = new SeedDataCommand.Handler(ActDb, Mock.Of<IItemsSource>(), CreateAppEnv(), CharacterService,
                ExperienceTable, StrategusMap, settlementsSource.Object, ItemValueModel,
                ItemModifierService);
            await handler.Handle(new SeedDataCommand(), CancellationToken.None);

            var settlements = await AssertDb.Settlements.ToArrayAsync();
            Assert.AreEqual(0, settlements.Length);
        }

        private IApplicationEnvironment CreateAppEnv()
        {
            var appEnv = new Mock<IApplicationEnvironment>();
            appEnv.Setup(e => e.Environment).Returns(HostingEnvironment.Production);
            return appEnv.Object;
        }
    }
}
