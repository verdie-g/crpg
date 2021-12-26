using Crpg.Application.Bans.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Bans;

public class BanCommandTest : TestBase
{
    [Test]
    public async Task BanExistingUser()
    {
        var user1 = ArrangeDb.Users.Add(new User());
        var user2 = ArrangeDb.Users.Add(new User { PlatformUserId = "1234", Name = "toto" });
        await ArrangeDb.SaveChangesAsync();

        var result = await new BanCommand.Handler(ActDb, Mapper).Handle(new BanCommand
        {
            BannedUserId = user1.Entity.Id,
            Duration = TimeSpan.FromDays(1),
            Reason = "toto",
            BannedByUserId = user2.Entity.Id,
        }, CancellationToken.None);

        var ban = result.Data!;
        Assert.AreEqual(user1.Entity.Id, ban.BannedUser!.Id);
        Assert.AreEqual(TimeSpan.FromDays(1), ban.Duration);
        Assert.AreEqual("toto", ban.Reason);
        Assert.AreEqual(user2.Entity.Id, ban.BannedByUser!.Id);
        Assert.AreEqual(user2.Entity.PlatformUserId, ban.BannedByUser.PlatformUserId);
        Assert.AreEqual(user2.Entity.Name, ban.BannedByUser.Name);
    }

    [Test]
    public async Task BanNonExistingUserShouldThrowNotFound()
    {
        var user2 = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var result = await new BanCommand.Handler(ActDb, Mapper).Handle(new BanCommand
        {
            BannedUserId = 10,
            Duration = TimeSpan.FromDays(1),
            Reason = "toto",
            BannedByUserId = user2.Entity.Id,
        }, CancellationToken.None);

        Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task BanByNonExistingUserShouldThrowNotFound()
    {
        var user1 = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var result = await new BanCommand.Handler(ActDb, Mapper).Handle(new BanCommand
        {
            BannedUserId = user1.Entity.Id,
            Duration = TimeSpan.FromDays(1),
            Reason = "toto",
            BannedByUserId = 10,
        }, CancellationToken.None);

        Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
    }

    [Test]
    public void BanningYourselfShouldReturnError()
    {
        var res = new BanCommand.Validator().Validate(new BanCommand
        {
            BannedUserId = 1,
            Duration = TimeSpan.Zero,
            Reason = "aaa",
            BannedByUserId = 1,
        });

        Assert.AreEqual(1, res.Errors.Count);
    }

    [Test]
    public void EmptyBanReasonShouldTrow()
    {
        var res = new BanCommand.Validator().Validate(new BanCommand
        {
            BannedUserId = 1,
            Duration = TimeSpan.Zero,
            Reason = string.Empty,
            BannedByUserId = 2,
        });

        Assert.AreEqual(1, res.Errors.Count);
    }
}
