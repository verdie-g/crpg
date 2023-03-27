using Crpg.Application.Common.Results;
using Crpg.Application.Users.Queries;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class GetUserByIdQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfUserDoesNotExist()
    {
        GetUserByIdQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserByIdQuery
        {
            UserId = 1,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task TestWhenUserExists()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        GetUserByIdQuery.Handler handler = new(ActDb, Mapper);
        var userVm = await handler.Handle(new GetUserByIdQuery
        {
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.That(userVm.Data, Is.Not.Null);
    }
}
