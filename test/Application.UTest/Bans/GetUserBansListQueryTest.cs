using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Bans.Queries;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Bans
{
    public class GetUserBansListQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user = new User
            {
                Bans = new List<Ban>
                {
                    new Ban { BannedByUser = new User { PlatformUserId = "123" } },
                    new Ban { BannedByUser = new User { PlatformUserId = "456" } },
                }
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var bans = await new GetUserBansListQuery.Handler(ActDb, Mapper).Handle(
                new GetUserBansListQuery { UserId = user.Id }, CancellationToken.None);
            Assert.AreEqual(2, bans.Count);
            Assert.AreEqual("123", bans[0].BannedByUser!.PlatformUserId);
            Assert.AreEqual("456", bans[1].BannedByUser!.PlatformUserId);
        }

        [Test]
        public void NotFoundUser()
        {
            var handler = new GetUserBansListQuery.Handler(ActDb, Mapper);
            Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(new GetUserBansListQuery { UserId = 1 }, CancellationToken.None));
        }
    }
}