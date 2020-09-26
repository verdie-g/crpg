using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class DeleteCharacterCommandTest : TestBase
    {
        [Test]
        public async Task WhenCharacterExists()
        {
            var e = ArrangeDb.Characters.Add(new Character
            {
                Name = "sword",
                UserId = 1,
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new DeleteCharacterCommand.Handler(ActDb);
            await handler.Handle(new DeleteCharacterCommand
            {
                CharacterId = e.Entity.Id,
                UserId = e.Entity.UserId,
            }, CancellationToken.None);

            Assert.IsNull(await AssertDb.Characters.FindAsync(e.Entity.Id));
        }

        [Test]
        public async Task WhenCharacterExistsButNotOwnedByUser()
        {
            var e = ArrangeDb.Characters.Add(new Character
            {
                Name = "sword",
                UserId = 2,
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new DeleteCharacterCommand.Handler(ActDb);
            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new DeleteCharacterCommand
            {
                CharacterId = e.Entity.Id,
                UserId = 1,
            }, CancellationToken.None));
        }

        [Test]
        public void WhenCharacterDoesntExist()
        {
            var handler = new DeleteCharacterCommand.Handler(ActDb);
            Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(request: new DeleteCharacterCommand { CharacterId = 1 }, CancellationToken.None));
        }
    }
}