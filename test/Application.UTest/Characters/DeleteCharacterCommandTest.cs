using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Bans.Commands;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class DeleteCharacterCommandTest : TestBase
    {
        private static readonly ILogger<DeleteCharacterCommand> Logger = Mock.Of<ILogger<DeleteCharacterCommand>>();

        [Test]
        public async Task WhenCharacterExists()
        {
            var e = ArrangeDb.Characters.Add(new Character
            {
                Name = "sword",
                UserId = 1,
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new DeleteCharacterCommand.Handler(ActDb, Logger);
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

            var handler = new DeleteCharacterCommand.Handler(ActDb, Logger);
            var result = await handler.Handle(new DeleteCharacterCommand
            {
                CharacterId = e.Entity.Id,
                UserId = 1,
            }, CancellationToken.None);

            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task WhenCharacterDoesntExist()
        {
            var handler = new DeleteCharacterCommand.Handler(ActDb, Logger);
            var result = await handler.Handle(new DeleteCharacterCommand { CharacterId = 1 }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }
    }
}
