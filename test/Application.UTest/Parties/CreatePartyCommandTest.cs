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
            Region = Region.Na,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
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
            Region = Region.Na,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.UserAlreadyRegisteredToStrategus, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldRegisterToStrategus()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock.Setup(sm => sm.GetSpawnPosition(Region.Na)).Returns(new Point(150, 50));
        CreatePartyCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object, Constants);
        var res = await handler.Handle(new CreatePartyCommand
        {
            UserId = user.Id,
            Region = Region.Na,
        }, CancellationToken.None);

        var party = res.Data!;
        Assert.IsNotNull(party);
        Assert.AreEqual(user.Id, party.Id);
        Assert.AreEqual(Region.Na, party.Region);
        Assert.AreEqual(0, party.Gold);
        Assert.AreEqual(1, party.Troops);
        Assert.AreEqual(new Point(150.0, 50.0), party.Position);
        Assert.AreEqual(PartyStatus.Idle, party.Status);
        Assert.AreEqual(0, party.Waypoints.Count);
        Assert.IsNull(party.TargetedParty);
        Assert.IsNull(party.TargetedSettlement);
    }
}
