﻿using Crpg.Application.Bans.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Bans;

public class GetUserBansQueryTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        User user = new()
        {
            Bans = new List<Ban>
            {
                new() { BannedByUser = new User { PlatformUserId = "123" } },
                new() { BannedByUser = new User { PlatformUserId = "456" } },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetUserBansQuery.Handler(ActDb, Mapper).Handle(
            new GetUserBansQuery { UserId = user.Id }, CancellationToken.None);
        var bans = result.Data!;
        Assert.AreEqual(2, bans.Count);
        Assert.AreEqual("123", bans[0].BannedByUser!.PlatformUserId);
        Assert.AreEqual("456", bans[1].BannedByUser!.PlatformUserId);
    }

    [Test]
    public async Task NotFoundUser()
    {
        GetUserBansQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserBansQuery { UserId = 1 }, CancellationToken.None);
        Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
    }
}
