using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans
{
    public class GetUserClanQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfClanDoesntExist()
        {
            GetUserClanQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetUserClanQuery
            {
                UserId = 1,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnNullIfUserNotInAClan()
        {
            User user = new();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            GetUserClanQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetUserClanQuery
            {
                UserId = user.Id,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.IsNull(res.Data);
        }

        [Test]
        public async Task ShouldReturnClanIfUserInAClan()
        {
            User user = new() { ClanMembership = new ClanMember { Clan = new Clan() } };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            GetUserClanQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetUserClanQuery
            {
                UserId = user.Id,
            }, CancellationToken.None);

            var clan = res.Data!;
            Assert.IsNull(res.Errors);
            Assert.AreEqual(user.ClanMembership.ClanId, clan.Id);
        }
    }
}
