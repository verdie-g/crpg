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
        Assert.That(restriction.RestrictedUser!.Id, Is.EqualTo(user1.Entity.Id));
        Assert.That(restriction.Duration, Is.EqualTo(TimeSpan.FromDays(1)));
        Assert.That(restriction.Reason, Is.EqualTo("toto"));
        Assert.That(restriction.RestrictedByUser!.Id, Is.EqualTo(user2.Entity.Id));
        Assert.That(restriction.RestrictedByUser.PlatformUserId, Is.EqualTo(user2.Entity.PlatformUserId));
        Assert.That(restriction.RestrictedByUser.Name, Is.EqualTo(user2.Entity.Name));
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

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
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

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
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

        Assert.That(res.Errors.Count, Is.EqualTo(1));
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

        Assert.That(res.Errors.Count, Is.EqualTo(1));
    }
}
