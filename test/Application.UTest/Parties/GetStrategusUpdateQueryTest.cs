using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Queries;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Parties;

public class GetStrategusUpdateQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfNotFound()
    {
        GetStrategusUpdateQuery.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new GetStrategusUpdateQuery
        {
            PartyId = 1,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfNotRegisteredToStrategus()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        GetStrategusUpdateQuery.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new GetStrategusUpdateQuery
        {
            PartyId = user.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnPartyWithWhatsVisible()
    {
        Party party = new()
        {
            Position = new Point(10, 10),
            User = new User(),
        };
        Party closeParty = new()
        {
            Position = new Point(9.9, 9.9),
            User = new User(),
        };
        Party closePartyInBattle = new()
        {
            Position = new Point(9.8, 9.8),
            User = new User(),
            Status = PartyStatus.InBattle,
        };
        Party farParty = new()
        {
            Position = new Point(1000, 1000),
            User = new User(),
        };
        Party partyInSettlement = new()
        {
            Position = new Point(10.1, 10.1),
            Status = PartyStatus.IdleInSettlement,
            User = new User(),
        };
        ArrangeDb.Parties.AddRange(party, closeParty, farParty, partyInSettlement, closePartyInBattle);

        Settlement closeSettlement = new() { Position = new Point(10.1, 10.1) };
        Settlement farSettlement = new() { Position = new Point(-1000, -1000) };
        ArrangeDb.Settlements.AddRange(closeSettlement, farSettlement);
        await ArrangeDb.SaveChangesAsync();

        Battle closeBattle = new() { Position = new Point(9.0, 9.0) };
        Battle closeEndedBattle = new()
        {
            Position = new Point(8.0, 8.0),
            Phase = BattlePhase.End,
        };
        Battle farBattle = new() { Position = new Point(-999, -999) };
        ArrangeDb.Battles.AddRange(closeBattle, closeEndedBattle, farBattle);
        await ArrangeDb.SaveChangesAsync();

        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock.Setup(m => m.ViewDistance).Returns(50);

        GetStrategusUpdateQuery.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new GetStrategusUpdateQuery
        {
            PartyId = party.Id,
        }, CancellationToken.None);

        var update = res.Data!;
        Assert.IsNotNull(update);
        Assert.NotNull(update.Party);
        Assert.AreEqual(1, update.VisibleParties.Count);
        Assert.AreEqual(closeParty.Id, update.VisibleParties[0].Id);
        Assert.AreEqual(1, update.VisibleSettlements.Count);
        Assert.AreEqual(closeSettlement.Id, update.VisibleSettlements[0].Id);
        Assert.AreEqual(1, update.VisibleBattles.Count);
        Assert.AreEqual(closeBattle.Id, update.VisibleBattles[0].Id);
    }
}
