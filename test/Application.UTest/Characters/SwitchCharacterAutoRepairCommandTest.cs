using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class SwitchCharacterAutoRepairCommandTest : TestBase
    {
        [Test]
        public async Task ShouldSwitchOnAutoRepairWithTrue()
        {
            Character character = new() { AutoRepair = false };
            ArrangeDb.Characters.Add(character);
            await ArrangeDb.SaveChangesAsync();

            var result = await new SwitchCharacterAutoRepairCommand.Handler(ActDb).Handle(
                new SwitchCharacterAutoRepairCommand
                {
                    CharacterId = character.Id,
                    UserId = character.UserId,
                    AutoRepair = true,
                }, CancellationToken.None);

            Assert.IsNull(result.Errors);
            character = AssertDb.Characters.First(c => c.Id == character.Id);
            Assert.IsTrue(character.AutoRepair);
        }

        [Test]
        public async Task ShouldSwitchOffAutoRepairWithFalse()
        {
            Character character = new() { AutoRepair = true };
            ArrangeDb.Characters.Add(character);
            await ArrangeDb.SaveChangesAsync();

            var result = await new SwitchCharacterAutoRepairCommand.Handler(ActDb).Handle(
                new SwitchCharacterAutoRepairCommand
                {
                    CharacterId = character.Id,
                    UserId = character.UserId,
                    AutoRepair = false,
                }, CancellationToken.None);

            Assert.IsNull(result.Errors);
            character = AssertDb.Characters.First(c => c.Id == character.Id);
            Assert.IsFalse(character.AutoRepair);
        }

        [Test]
        public async Task ShouldThrowNotFoundIfCharacterNotFound()
        {
            User user = new();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            SwitchCharacterAutoRepairCommand.Handler handler = new(ActDb);
            var result = await handler.Handle(new SwitchCharacterAutoRepairCommand
            {
                UserId = user.Id,
                CharacterId = 1,
                AutoRepair = true,
            }, CancellationToken.None);

            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldThrowNotFoundIfUserNotFound()
        {
            SwitchCharacterAutoRepairCommand.Handler handler = new(ActDb);
            var result = await handler.Handle(new SwitchCharacterAutoRepairCommand
            {
                UserId = 1,
                CharacterId = 1,
            }, CancellationToken.None);

            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }
    }
}
