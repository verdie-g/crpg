using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Queries;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class GetUserCharacterListQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            Db.AddRange(
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
            await Db.SaveChangesAsync();

            var handler = new GetUserCharactersListQuery.Handler(Db, Mapper);
            var characters = await handler.Handle(new GetUserCharactersListQuery { UserId = 1 }, CancellationToken.None);

            Assert.AreEqual(2, characters.Count);
        }
    }
}