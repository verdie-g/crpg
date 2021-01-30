using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Clans.Queries;
using Crpg.Domain.Entities.Clans;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans
{
    public class GetClansQueryTest : TestBase
    {
        [Test]
        public async Task ShouldGetClans()
        {
            ArrangeDb.Clans.AddRange(new Clan(), new Clan());
            await ArrangeDb.SaveChangesAsync();

            var result = await new GetClansQuery.Handler(ActDb, Mapper).Handle(new GetClansQuery(), CancellationToken.None);
            Assert.AreEqual(2, result.Data!.Count);
        }
    }
}
