using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;

namespace Crpg.Application.UTest.Characters
{
    public class DeleteCharacterCommandTest : TestBase
    {
        [Test]
        public async Task WhenCharacterExists()
        {
            var e = _db.Characters.Add(new Character
            {
                Name = "sword",
                UserId = 1,
            });
            await _db.SaveChangesAsync();

            var handler = new DeleteCharacterCommand.Handler(_db);
            await handler.Handle(new DeleteCharacterCommand
            {
                CharacterId = e.Entity.Id,
                UserId = e.Entity.UserId,
            }, CancellationToken.None);

            Assert.IsNull(await _db.Characters.FindAsync(e.Entity.Id));
        }

        [Test]
        public async Task WhenCharacterExistsButNotOwnedByUser()
        {
            var e = _db.Characters.Add(new Character
            {
                Name = "sword",
                UserId = 2,
            });
            await _db.SaveChangesAsync();

            var handler = new DeleteCharacterCommand.Handler(_db);
            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new DeleteCharacterCommand
            {
                CharacterId = e.Entity.Id,
                UserId = 1,
            }, CancellationToken.None));
        }

        [Test]
        public void WhenCharacterDoesntExist()
        {
            var handler = new DeleteCharacterCommand.Handler(_db);
            Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(new DeleteCharacterCommand {CharacterId = 1}, CancellationToken.None));
        }
    }
}