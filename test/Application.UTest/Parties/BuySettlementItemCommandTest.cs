using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Parties;

public class BuySettlementItemCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyNotFound()
    {
        BuySettlementItemCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new BuySettlementItemCommand
        {
            PartyId = 1,
            ItemId = 2,
            ItemCount = 1,
            SettlementId = 3,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfSettlementNotFound()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        BuySettlementItemCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new BuySettlementItemCommand
        {
            PartyId = party.Id,
            ItemId = 2,
            ItemCount = 1,
            SettlementId = 3,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.SettlementNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfSettlementTooFar()
    {
        Point userPosition = new(1, 2);
        Point settlementPosition = new(3, 4);

        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(userPosition, settlementPosition))
            .Returns(false);

        Party party = new() { Position = userPosition, User = new User() };
        ArrangeDb.Parties.Add(party);
        Settlement settlement = new() { Position = settlementPosition };
        ArrangeDb.Settlements.Add(settlement);
        await ArrangeDb.SaveChangesAsync();

        BuySettlementItemCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new BuySettlementItemCommand
        {
            PartyId = party.Id,
            ItemId = 2,
            ItemCount = 1,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.SettlementTooFar, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfItemNotFound()
    {
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(It.IsAny<Point>(), It.IsAny<Point>()))
            .Returns(true);

        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);
        await ArrangeDb.SaveChangesAsync();

        BuySettlementItemCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new BuySettlementItemCommand
        {
            PartyId = party.Id,
            ItemId = 2,
            ItemCount = 1,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ItemNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfItemNotRank0()
    {
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(It.IsAny<Point>(), It.IsAny<Point>()))
            .Returns(true);

        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);
        Settlement settlement = new() { Culture = Culture.Aserai };
        ArrangeDb.Settlements.Add(settlement);
        Item item = new() { Rank = 1, Culture = Culture.Aserai };
        ArrangeDb.Items.Add(item);
        await ArrangeDb.SaveChangesAsync();

        BuySettlementItemCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new BuySettlementItemCommand
        {
            PartyId = party.Id,
            ItemId = item.Id,
            ItemCount = 1,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ItemNotBuyable, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfItemIsNotFromTheSettlementCulture()
    {
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(It.IsAny<Point>(), It.IsAny<Point>()))
            .Returns(true);

        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);
        Settlement settlement = new() { Culture = Culture.Aserai };
        ArrangeDb.Settlements.Add(settlement);
        Item item = new() { Rank = 0, Culture = Culture.Empire };
        ArrangeDb.Items.Add(item);
        await ArrangeDb.SaveChangesAsync();

        BuySettlementItemCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new BuySettlementItemCommand
        {
            PartyId = party.Id,
            ItemId = item.Id,
            ItemCount = 1,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ItemNotBuyable, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfNotEnoughGold()
    {
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(It.IsAny<Point>(), It.IsAny<Point>()))
            .Returns(true);

        Party party = new() { Gold = 21, User = new User() };
        ArrangeDb.Parties.Add(party);
        Settlement settlement = new() { Culture = Culture.Empire };
        ArrangeDb.Settlements.Add(settlement);
        Item item = new() { Rank = 0, Culture = Culture.Empire, Price = 11 };
        ArrangeDb.Items.Add(item);
        await ArrangeDb.SaveChangesAsync();

        BuySettlementItemCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new BuySettlementItemCommand
        {
            PartyId = party.Id,
            ItemId = item.Id,
            ItemCount = 2,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.NotEnoughGold, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldAddCountToAlreadyExistingPartyItems()
    {
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(It.IsAny<Point>(), It.IsAny<Point>()))
            .Returns(true);

        Party party = new() { Gold = 100, User = new User() };
        ArrangeDb.Parties.Add(party);
        Settlement settlement = new() { Culture = Culture.Sturgia };
        ArrangeDb.Settlements.Add(settlement);
        Item item = new() { Rank = 0, Culture = Culture.Sturgia, Price = 10 };
        ArrangeDb.Items.Add(item);
        PartyItem partyItem = new() { Item = item, Count = 3, Party = party };
        ArrangeDb.PartyItems.Add(partyItem);
        await ArrangeDb.SaveChangesAsync();

        BuySettlementItemCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new BuySettlementItemCommand
        {
            PartyId = party.Id,
            ItemId = item.Id,
            ItemCount = 4,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        Assert.AreEqual(item.Id, res.Data!.Item.Id);
        Assert.AreEqual(7, res.Data!.Count);

        party = await AssertDb.Parties.FirstAsync(h => h.Id == party.Id);
        Assert.AreEqual(60, party.Gold);
    }

    [Test]
    public async Task ShouldAddItemToParty()
    {
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(It.IsAny<Point>(), It.IsAny<Point>()))
            .Returns(true);

        Party party = new() { Gold = 100, User = new User() };
        ArrangeDb.Parties.Add(party);
        Settlement settlement = new() { Culture = Culture.Sturgia };
        ArrangeDb.Settlements.Add(settlement);
        Item item = new() { Rank = 0, Culture = Culture.Neutral, Price = 10 };
        ArrangeDb.Items.Add(item);
        await ArrangeDb.SaveChangesAsync();

        BuySettlementItemCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new BuySettlementItemCommand
        {
            PartyId = party.Id,
            ItemId = item.Id,
            ItemCount = 10,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        Assert.AreEqual(item.Id, res.Data!.Item.Id);
        Assert.AreEqual(10, res.Data!.Count);

        party = await AssertDb.Parties.FirstAsync(h => h.Id == party.Id);
        Assert.AreEqual(0, party.Gold);
    }
}
