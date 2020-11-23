using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
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
                BodyProperties = "000AAC080000100DB976648E6774B835537D86629511323BDCB177278A84F667000776030048B49500000000000000000000000000000000000000003E045002",
                Gender = CharacterGender.Male,
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
                BodyProperties = "000AAC080000100DB976648795577464537D86629511323BDCB177278A84F667007776030748B49500000000000000000000000000000000000000003EFC5002",
                Gender = CharacterGender.Female,
            };

            var result = await new UpdateCharacterCommand.Handler(ActDb, Mapper).Handle(cmd, CancellationToken.None);
            Assert.AreEqual(cmd.Name, result.Data!.Name);
            Assert.AreEqual(cmd.BodyProperties, result.Data!.BodyProperties);
            Assert.AreEqual(cmd.Gender, result.Data!.Gender);
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
