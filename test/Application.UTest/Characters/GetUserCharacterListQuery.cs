using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Characters.Queries;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Characters
{
    public class GetUserCharacterListQuery : TestBase
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