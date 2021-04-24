using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Clans.Queries;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans
{
    public class GetClansQueryTest : TestBase
    {
        [Test]
        public async Task ShouldGetClans()
        {
            var clan1 = new Clan
            {
                Members = { new ClanMember { User = new User() } },
            };
            var clan2 = new Clan
            {
                Members = { new ClanMember { User = new User() }, new ClanMember { User = new User() } },
            };
            ArrangeDb.Clans.AddRange(clan1, clan2);
            await ArrangeDb.SaveChangesAsync();

            var result = await new GetClansQuery.Handler(ActDb, Mapper).Handle(new GetClansQuery(), CancellationToken.None);
            var clans = result.Data!;
            Assert.AreEqual(2, clans.Count);
            Assert.AreEqual(2, clans[0].MemberCount);
            Assert.AreEqual(1, clans[1].MemberCount);
        }
    }
}
