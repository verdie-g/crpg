using System.Net;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class UpdateUserRegionCommandTest : TestBase
{
    [Test]
    public async Task UserNotFound()
    {
        UpdateUserRegionCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IGeoIpService>());
        var res = await handler.Handle(new UpdateUserRegionCommand
        {
            UserId = 1,
            IpAddress = IPAddress.Any,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task IpAddressResolved()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IGeoIpService> geoIpServiceMock = new();
        geoIpServiceMock
            .Setup(s => s.ResolveRegionFromIp(IPAddress.Any))
            .Returns(Region.As);

        UpdateUserRegionCommand.Handler handler = new(ActDb, Mapper, geoIpServiceMock.Object);
        var res = await handler.Handle(new UpdateUserRegionCommand
        {
            UserId = user.Id,
            IpAddress = IPAddress.Any,
        }, CancellationToken.None);

        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.Region, Is.EqualTo(Region.As));
    }

    [Test]
    public async Task IpAddressNotResolved()
    {
        User user = new() { Region = Region.Na };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IGeoIpService> geoIpServiceMock = new();
        geoIpServiceMock
            .Setup(s => s.ResolveRegionFromIp(IPAddress.Any))
            .Returns((Region?)null);

        UpdateUserRegionCommand.Handler handler = new(ActDb, Mapper, geoIpServiceMock.Object);
        var res = await handler.Handle(new UpdateUserRegionCommand
        {
            UserId = user.Id,
            IpAddress = IPAddress.Any,
        }, CancellationToken.None);

        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.Region, Is.EqualTo(Region.Na));
    }
}
