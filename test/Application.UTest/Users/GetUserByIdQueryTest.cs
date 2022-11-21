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
        Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
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

        Assert.NotNull(userVm.Data);
    }
}
