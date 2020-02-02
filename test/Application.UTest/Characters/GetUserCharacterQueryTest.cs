using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Characters.Queries;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Characters
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
            var equipment = await handler.Handle(new GetUserCharacterQuery
            {
                CharacterId = dbCharacter.Id,
                UserId = 2,
            }, CancellationToken.None);

            Assert.NotNull(equipment);
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