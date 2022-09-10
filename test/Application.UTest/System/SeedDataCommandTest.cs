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

namespace Crpg.Application.UTest.System;

public class SeedDataCommandTest : TestBase
{
    private static readonly Region[] Regions = Enum.GetValues(typeof(Region)).Cast<Region>().ToArray();
    private static readonly IExperienceTable ExperienceTable = Mock.Of<IExperienceTable>();
    private static readonly ICharacterService CharacterService = Mock.Of<ICharacterService>();
    private static readonly IStrategusMap StrategusMap = Mock.Of<IStrategusMap>();
    private static readonly ItemModifiers ItemModifiers = new FileItemModifiersSource().LoadItemModifiers();
    private static readonly ItemModifierService ItemModifierService = new(ItemModifiers);

    [Test]
    public async Task ShouldInsertItemsFromItemSource()
    {
        Mock<IItemsSource> itemsSource = new();
        itemsSource.Setup(s => s.LoadItems())
            .ReturnsAsync(new[]
            {
                new ItemCreation { Id = "a", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel() },
                new ItemCreation { Id = "b", Type = ItemType.HeadArmor, Armor = new ItemArmorComponentViewModel() },
            });

        SeedDataCommand.Handler seedDataCommandHandler = new(ActDb, itemsSource.Object, CreateAppEnv(),
            CharacterService, ExperienceTable, StrategusMap, Mock.Of<ISettlementsSource>(),
            ItemModifierService);
        await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);

        var items = await AssertDb.Items.ToArrayAsync();
        Assert.AreEqual(2, items.Length);
    }

    [Test]
    public async Task DeletedItemInSourceShouldBeDeletedInDatabaseAndUserReimbursed()
    {
        Mock<IItemsSource> itemsSource = new();
        itemsSource.SetupSequence(s => s.LoadItems())
            .ReturnsAsync(new[] { new ItemCreation { Id = "a", Type = ItemType.HeadArmor, Price = 20, Armor = new ItemArmorComponentViewModel() } })
            .ReturnsAsync(Array.Empty<ItemCreation>());

        SeedDataCommand.Handler seedDataCommandHandler = new(ActDb, itemsSource.Object, CreateAppEnv(),
            CharacterService, ExperienceTable, StrategusMap, Mock.Of<ISettlementsSource>(),
            ItemModifierService);
        await seedDataCommandHandler.Handle(new SeedDataCommand(), CancellationToken.None);
        var items = await AssertDb.Items.ToArrayAsync();
        Assert.AreEqual(1, items.Length);

        // Users buy the new item and equip it.
        User user0 = new() { Gold = 100, HeirloomPoints = 0 };
        UserItem userItemRank0ForUser0 = new() { BaseItemId = items.First().Id, Rank = 0, User = user0 };
        Character character0 = new()
        {
            User = user0,
            EquippedItems =
            {
                new EquippedItem { UserItem = userItemRank0ForUser0, Slot = ItemSlot.Weapon0 },
                new EquippedItem { UserItem = userItemRank0ForUser0, Slot = ItemSlot.Weapon1 },
            },
        };

        User user1 = new() { Gold = 200, HeirloomPoints = 0 };
        UserItem userItemRank0ForUser1 = new() { BaseItemId = items.First().Id, Rank = 0, User = user1 };
        UserItem userItemRank1ForUser1 = new() { BaseItemId = items.First().Id, Rank = 1, User = user1 };
        Character character1 = new()
        {
            User = user1,
            EquippedItems = { new EquippedItem { UserItem = userItemRank0ForUser1, Slot = ItemSlot.Weapon0 } },
        };

        Character character2 = new()
        {
            User = user1,
            EquippedItems = { new EquippedItem { UserItem = userItemRank1ForUser1, Slot = ItemSlot.Weapon1 } },
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
    public async Task ShouldAddSettlementIfDoesntExistsInDb()
    {
        Mock<ISettlementsSource> settlementsSource = new();
        settlementsSource.Setup(s => s.LoadStrategusSettlements())
            .ReturnsAsync(new[]
            {
                new SettlementCreation { Name = "a", Position = new Point(0, 0) },
                new SettlementCreation { Name = "b", Position = new Point(0, 0) },
            });

        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.TranslatePositionForRegion(It.IsAny<Point>(), It.IsAny<Region>(), It.IsAny<Region>()))
            .Returns((Point point, Region _, Region _) => point);

        SeedDataCommand.Handler handler = new(ActDb, Mock.Of<IItemsSource>(), CreateAppEnv(), CharacterService,
            ExperienceTable, strategusMapMock.Object, settlementsSource.Object, ItemModifierService);
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
        Settlement[] dbSettlements =
        {
            new()
            {
                Name = "a",
                Type = SettlementType.Castle,
                Culture = Culture.Aserai,
                Region = Region.Europe,
                Position = new Point(1, 2),
                Scene = "abc",
            },
            new()
            {
                Name = "a",
                Type = SettlementType.Castle,
                Culture = Culture.Aserai,
                Region = Region.NorthAmerica,
                Position = new Point(1, 2),
                Scene = "abc",
            },
            new()
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

        Mock<ISettlementsSource> settlementsSource = new();
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

        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.TranslatePositionForRegion(It.IsAny<Point>(), Region.Europe, Region.Europe))
            .Returns(new Point(3, 4));
        strategusMapMock
            .Setup(m => m.TranslatePositionForRegion(It.IsAny<Point>(), Region.Europe, Region.NorthAmerica))
            .Returns(new Point(4, 5));
        strategusMapMock
            .Setup(m => m.TranslatePositionForRegion(It.IsAny<Point>(), Region.Europe, Region.Asia))
            .Returns(new Point(5, 6));

        SeedDataCommand.Handler handler = new(ActDb, Mock.Of<IItemsSource>(), CreateAppEnv(), CharacterService,
            ExperienceTable, strategusMapMock.Object, settlementsSource.Object, ItemModifierService);
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

        Mock<ISettlementsSource> settlementsSource = new();
        settlementsSource.Setup(s => s.LoadStrategusSettlements())
            .ReturnsAsync(Array.Empty<SettlementCreation>());

        SeedDataCommand.Handler handler = new(ActDb, Mock.Of<IItemsSource>(), CreateAppEnv(), CharacterService,
            ExperienceTable, StrategusMap, settlementsSource.Object, ItemModifierService);
        await handler.Handle(new SeedDataCommand(), CancellationToken.None);

        var settlements = await AssertDb.Settlements.ToArrayAsync();
        Assert.AreEqual(0, settlements.Length);
    }

    private IApplicationEnvironment CreateAppEnv()
    {
        Mock<IApplicationEnvironment> appEnv = new();
        appEnv.Setup(e => e.Environment).Returns(HostingEnvironment.Production);
        return appEnv.Object;
    }
}
