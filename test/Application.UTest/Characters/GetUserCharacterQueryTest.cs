using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class GetUserCharacterQueryTest : TestBase
    {
        [Test]
        public void WhenCharacterDoesntExist()
        {
            var handler = new GetUserCharacterQuery.Handler(ActDb, Mapper);
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
            ArrangeDb.Characters.Add(dbCharacter);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetUserCharacterQuery.Handler(ActDb, Mapper);
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
            ArrangeDb.Characters.Add(dbCharacter);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetUserCharacterQuery.Handler(ActDb, Mapper);
            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetUserCharacterQuery
            {
                CharacterId = dbCharacter.Id,
                UserId = 1,
            }, CancellationToken.None));
        }
    }
}