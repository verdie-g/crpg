﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Bans.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Bans
{
    public class GetBansQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user1 = new User();
            var user2 = new User();

            var bans = new List<Ban>
            {
                new() { BannedUser = user1, BannedByUser = user2 },
                new() { BannedUser = user2, BannedByUser = user1 },
            };
            ArrangeDb.Bans.AddRange(bans);
            await ArrangeDb.SaveChangesAsync();

            var result = await new GetBansQuery.Handler(ActDb, Mapper).Handle(
                new GetBansQuery(), CancellationToken.None);
            Assert.AreEqual(2, result.Data!.Count);
        }
    }
}
