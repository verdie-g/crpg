using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class UpdateCharacterCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var character = ArrangeDb.Characters.Add(new Character
            {
                Name = "toto",
            });
            var user = ArrangeDb.Users.Add(new User
            {
                Characters = new List<Character> { character.Entity },
            });
            await ArrangeDb.SaveChangesAsync();

            var cmd = new UpdateCharacterCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                Name = "tata",
            };

            var result = await new UpdateCharacterCommand.Handler(ActDb, Mapper).Handle(cmd, CancellationToken.None);
            Assert.AreEqual(cmd.Name, result.Data!.Name);
        }

        [Test]
        public async Task CharacterNotFound()
        {
            var user = ArrangeDb.Users.Add(new User());
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterCommand.Handler(ActDb, Mapper);
            var cmd = new UpdateCharacterCommand
            {
                CharacterId = 1,
                UserId = user.Entity.Id,
            };

            var result = await handler.Handle(cmd, CancellationToken.None);
            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task CharacterNotOwned()
        {
            var character = ArrangeDb.Characters.Add(new Character());
            var user = ArrangeDb.Users.Add(new User());
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterCommand.Handler(ActDb, Mapper);
            var cmd = new UpdateCharacterCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
            };

            var result = await handler.Handle(cmd, CancellationToken.None);
            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task UserNotFound()
        {
            var character = ArrangeDb.Characters.Add(new Character());
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterCommand.Handler(ActDb, Mapper);
            var cmd = new UpdateCharacterCommand
            {
                CharacterId = character.Entity.Id,
                UserId = 1,
            };

            var result = await handler.Handle(cmd, CancellationToken.None);
            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }
    }
}
