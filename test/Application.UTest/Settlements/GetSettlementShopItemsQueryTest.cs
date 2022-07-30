using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Settlements.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements;

public class GetSettlementShopItemsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfUserNotFound()
    {
        GetSettlementShopItemsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new GetSettlementShopItemsQuery
        {
            PartyId = 1,
            SettlementId = 2,
        }, CancellationToken.None);

        Assert.NotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfSettlementNotFound()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        GetSettlementShopItemsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new GetSettlementShopItemsQuery
        {
            PartyId = party.Id,
            SettlementId = 2,
        }, CancellationToken.None);

        Assert.NotNull(res.Errors);
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

        GetSettlementShopItemsQuery.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new GetSettlementShopItemsQuery
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.NotNull(res.Errors);
        Assert.AreEqual(ErrorCode.SettlementTooFar, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnItemsForTheSettlementCulture()
    {
        Point userPosition = new(1, 2);
        Point settlementPosition = new(3, 4);

        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(userPosition, settlementPosition))
            .Returns(true);

        Party party = new() { Position = userPosition, User = new User() };
        ArrangeDb.Parties.Add(party);
        Settlement settlement = new() { Position = settlementPosition, Culture = Culture.Battania };
        ArrangeDb.Settlements.Add(settlement);
        Item[] items =
        {
            new() { Id = "0", Culture = Culture.Aserai },
            new() { Id = "1", Culture = Culture.Battania },
            new() { Id = "2", Culture = Culture.Battania },
        };
        ArrangeDb.Items.AddRange(items);
        await ArrangeDb.SaveChangesAsync();

        GetSettlementShopItemsQuery.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new GetSettlementShopItemsQuery
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.Null(res.Errors);
        Assert.AreEqual(2, res.Data!.Count);
    }
}
