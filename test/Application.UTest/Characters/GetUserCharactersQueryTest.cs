using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Queries;
using Crpg.Domain.Entities.Characters;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class GetUserCharactersQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            ArrangeDb.AddRange(
                new Character
                {
                    Name = "toto",
                    UserId = 1,
                },
                new Character
                {
                    Name = "titi",
                    UserId = 1,
                },
                new Character
                {
                    Name = "tata",
                    UserId = 2,
                });
            await ArrangeDb.SaveChangesAsync();

            GetUserCharactersQuery.Handler handler = new(ActDb, Mapper);
            var result = await handler.Handle(new GetUserCharactersQuery { UserId = 1 }, CancellationToken.None);

            var characters = result.Data!;
            Assert.AreEqual(2, characters.Count);
        }
    }
}
