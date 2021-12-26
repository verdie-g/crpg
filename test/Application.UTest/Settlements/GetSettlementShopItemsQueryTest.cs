using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Settlements.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Items;
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
            HeroId = 1,
            SettlementId = 2,
        }, CancellationToken.None);

        Assert.NotNull(res.Errors);
        Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfSettlementNotFound()
    {
        Hero hero = new() { User = new User() };
        ArrangeDb.Heroes.Add(hero);
        await ArrangeDb.SaveChangesAsync();

        GetSettlementShopItemsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new GetSettlementShopItemsQuery
        {
            HeroId = hero.Id,
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

        Hero hero = new() { Position = userPosition, User = new User() };
        ArrangeDb.Heroes.Add(hero);
        Settlement settlement = new() { Position = settlementPosition };
        ArrangeDb.Settlements.Add(settlement);
        await ArrangeDb.SaveChangesAsync();

        GetSettlementShopItemsQuery.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new GetSettlementShopItemsQuery
        {
            HeroId = hero.Id,
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

        Hero hero = new() { Position = userPosition, User = new User() };
        ArrangeDb.Heroes.Add(hero);
        Settlement settlement = new() { Position = settlementPosition, Culture = Culture.Battania };
        ArrangeDb.Settlements.Add(settlement);
        Item[] items =
        {
            new() { Culture = Culture.Aserai, Rank = 0 },
            new() { Culture = Culture.Aserai, Rank = 1 },
            new() { Culture = Culture.Battania, Rank = 2 },
            new() { Culture = Culture.Battania, Rank = 0 },
            new() { Culture = Culture.Battania, Rank = 0 },
        };
        ArrangeDb.Items.AddRange(items);
        await ArrangeDb.SaveChangesAsync();

        GetSettlementShopItemsQuery.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new GetSettlementShopItemsQuery
        {
            HeroId = hero.Id,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.Null(res.Errors);
        Assert.AreEqual(2, res.Data!.Count);
    }
}
