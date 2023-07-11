using Crpg.Application.Common.Results;
using Crpg.Application.Users.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class UpdateUserCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorOnNotFoundUser()
    {
        var res = await new UpdateUserCommand.Handler(ActDb, Mapper).Handle(new UpdateUserCommand
        {
            UserId = 1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task ShouldUpdateUser()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await new UpdateUserCommand.Handler(ActDb, Mapper).Handle(new UpdateUserCommand
        {
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
    }
}
