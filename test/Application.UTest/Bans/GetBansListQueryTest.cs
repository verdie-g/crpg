using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Bans.Queries;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Bans
{
    public class GetBansListQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user1 = new User();
            var user2 = new User();

            var bans = new List<Ban>
            {
                new Ban { BannedUser = user1, BannedByUser = user2 },
                new Ban { BannedUser = user2, BannedByUser = user1 },
            };
            Db.Bans.AddRange(bans);
            await Db.SaveChangesAsync();

            var dbBans = await new GetBansListQuery.Handler(Db, Mapper).Handle(
                new GetBansListQuery(), CancellationToken.None);
            Assert.AreEqual(2, dbBans.Count);
        }
    }
}