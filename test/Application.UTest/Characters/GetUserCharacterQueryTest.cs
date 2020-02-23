using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Characters.Queries;
using Crpg.Domain.Entities;

namespace Crpg.Application.UTest.Characters
{
    public class GetUserCharacterQueryTest : TestBase
    {
        [Test]
        public void WhenCharacterDoesntExist()
        {
            var handler = new GetUserCharacterQuery.Handler(_db, _mapper);
            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetUserCharacterQuery
            {
                CharacterId = 1,
                UserId = 2,
            }, CancellationToken.None));
        }

        [Test]
        public async Task WhenCharacterExists()
        {
            var dbCharacter = new Character
            {
                Name = "toto",
                UserId = 2,
            };
            _db.Characters.Add(dbCharacter);
            await _db.SaveChangesAsync();

            var handler = new GetUserCharacterQuery.Handler(_db, _mapper);
            var item = await handler.Handle(new GetUserCharacterQuery
            {
                CharacterId = dbCharacter.Id,
                UserId = 2,
            }, CancellationToken.None);

            Assert.NotNull(item);
        }

        [Test]
        public async Task WhenCharacterExistsButNotOwned()
        {
            var dbCharacter = new Character
            {
                Name = "toto",
                UserId = 2,
            };
            _db.Characters.Add(dbCharacter);
            await _db.SaveChangesAsync();

            var handler = new GetUserCharacterQuery.Handler(_db, _mapper);
            Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new GetUserCharacterQuery
            {
                CharacterId = dbCharacter.Id,
                UserId = 1,
            }, CancellationToken.None));
        }
    }
}