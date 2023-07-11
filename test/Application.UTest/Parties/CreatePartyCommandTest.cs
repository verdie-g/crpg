using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Parties;

public class CreatePartyCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        StrategusMinPartyTroops = 1,
    };

    [Test]
    public async Task ShouldReturnErrorIfNotFound()
    {
        CreatePartyCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>(), Constants);
        var res = await handler.Handle(new CreatePartyCommand
        {
            UserId = 1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfAlreadyRegistered()
    {
        User user = new()
        {
            Party = new Party(),
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        CreatePartyCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>(), Constants);
        var res = await handler.Handle(new CreatePartyCommand
        {
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserAlreadyRegisteredToStrategus));
    }

    [Test]
    public async Task ShouldRegisterToStrategus()
    {
        User user = new() { Region = Region.Na };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock.Setup(sm => sm.GetSpawnPosition(Region.Na)).Returns(new Point(150, 50));
        CreatePartyCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object, Constants);
        var res = await handler.Handle(new CreatePartyCommand
        {
            UserId = user.Id,
        }, CancellationToken.None);

        var party = res.Data!;
        Assert.That(party, Is.Not.Null);
        Assert.That(party.Id, Is.EqualTo(user.Id));
        Assert.That(party.Gold, Is.EqualTo(0));
        Assert.That(party.Troops, Is.EqualTo(1));
        Assert.That(party.Position, Is.EqualTo(new Point(150.0, 50.0)));
        Assert.That(party.Status, Is.EqualTo(PartyStatus.Idle));
        Assert.That(party.Waypoints.Count, Is.EqualTo(0));
        Assert.That(party.TargetedParty, Is.Null);
        Assert.That(party.TargetedSettlement, Is.Null);
    }
}
