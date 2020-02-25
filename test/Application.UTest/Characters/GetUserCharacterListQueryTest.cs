using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Crpg.Application.Characters.Queries;
using Crpg.Domain.Entities;

namespace Crpg.Application.UTest.Characters
{
    public class GetUserCharacterListQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            _db.AddRange(
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
            await _db.SaveChangesAsync();

            var handler = new GetUserCharactersListQuery.Handler(_db, _mapper);
            var characters = await handler.Handle(new GetUserCharactersListQuery {UserId = 1}, CancellationToken.None);

            Assert.AreEqual(2, characters.Count);
        }
    }
}