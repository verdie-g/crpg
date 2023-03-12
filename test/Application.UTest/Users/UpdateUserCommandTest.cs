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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldUpdateUser()
    {
        User user = new() { Region = null };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var res = await new UpdateUserCommand.Handler(ActDb, Mapper).Handle(new UpdateUserCommand
        {
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        // Assert.AreEqual(Region.Na, res.Data!.Region);
    }
}
