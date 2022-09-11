using Crpg.Application.Common.Results;
using Crpg.Application.Restrictions.Commands;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Restrictions;

public class RestrictCommandTest : TestBase
{
    [Test]
    public async Task RestrictExistingUser()
    {
        var user1 = ArrangeDb.Users.Add(new User());
        var user2 = ArrangeDb.Users.Add(new User { PlatformUserId = "1234", Name = "toto" });
        await ArrangeDb.SaveChangesAsync();

        var result = await new RestrictCommand.Handler(ActDb, Mapper).Handle(new RestrictCommand
        {
            RestrictedUserId = user1.Entity.Id,
            Duration = TimeSpan.FromDays(1),
            Reason = "toto",
            RestrictedByUserId = user2.Entity.Id,
        }, CancellationToken.None);

        var restriction = result.Data!;
        Assert.AreEqual(user1.Entity.Id, restriction.RestrictedUser!.Id);
        Assert.AreEqual(TimeSpan.FromDays(1), restriction.Duration);
        Assert.AreEqual("toto", restriction.Reason);
        Assert.AreEqual(user2.Entity.Id, restriction.RestrictedByUser!.Id);
        Assert.AreEqual(user2.Entity.PlatformUserId, restriction.RestrictedByUser.PlatformUserId);
        Assert.AreEqual(user2.Entity.Name, restriction.RestrictedByUser.Name);
    }

    [Test]
    public async Task RestrictNonExistingUserShouldThrowNotFound()
    {
        var user2 = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var result = await new RestrictCommand.Handler(ActDb, Mapper).Handle(new RestrictCommand
        {
            RestrictedUserId = 10,
            Duration = TimeSpan.FromDays(1),
            Reason = "toto",
            RestrictedByUserId = user2.Entity.Id,
        }, CancellationToken.None);

        Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task RestrictedByNonExistingUserShouldThrowNotFound()
    {
        var user1 = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var result = await new RestrictCommand.Handler(ActDb, Mapper).Handle(new RestrictCommand
        {
            RestrictedUserId = user1.Entity.Id,
            Duration = TimeSpan.FromDays(1),
            Reason = "toto",
            RestrictedByUserId = 10,
        }, CancellationToken.None);

        Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
    }

    [Test]
    public void RestrictingYourselfShouldReturnError()
    {
        var res = new RestrictCommand.Validator().Validate(new RestrictCommand
        {
            RestrictedUserId = 1,
            Duration = TimeSpan.Zero,
            Reason = "aaa",
            RestrictedByUserId = 1,
        });

        Assert.AreEqual(1, res.Errors.Count);
    }

    [Test]
    public void EmptyRestrictionReasonReturnError()
    {
        var res = new RestrictCommand.Validator().Validate(new RestrictCommand
        {
            RestrictedUserId = 1,
            Duration = TimeSpan.Zero,
            Reason = string.Empty,
            RestrictedByUserId = 2,
        });

        Assert.AreEqual(1, res.Errors.Count);
    }
}
