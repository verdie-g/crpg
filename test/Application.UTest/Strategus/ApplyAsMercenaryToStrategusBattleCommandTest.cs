using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Commands;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Strategus.Battles;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class ApplyAsMercenaryToStrategusBattleCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfCharacterNotFound()
        {
            User user = new();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            ApplyAsMercenaryToStrategusBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new()
            {
                UserId = user.Id,
                CharacterId = 2,
                BattleId = 3,
                Side = StrategusBattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.CharacterNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfBattleNotFound()
        {
            Character character = new();
            User user = new() { Characters = { character } };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            ApplyAsMercenaryToStrategusBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new()
            {
                UserId = user.Id,
                CharacterId = character.Id,
                BattleId = 2,
                Side = StrategusBattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleNotFound, res.Errors![0].Code);
        }

        [TestCase(StrategusBattlePhase.Preparation)]
        [TestCase(StrategusBattlePhase.Battle)]
        [TestCase(StrategusBattlePhase.End)]
        public async Task ShouldReturnErrorIfBattleNotInHiringPhase(StrategusBattlePhase battlePhase)
        {
            Character character = new();
            User user = new() { Characters = { character } };
            ArrangeDb.Users.Add(user);

            StrategusBattle battle = new() { Phase = battlePhase };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            ApplyAsMercenaryToStrategusBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new()
            {
                UserId = user.Id,
                CharacterId = character.Id,
                BattleId = battle.Id,
                Side = StrategusBattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
        }

        [TestCase(StrategusBattleMercenaryApplicationStatus.Pending)]
        [TestCase(StrategusBattleMercenaryApplicationStatus.Accepted)]
        public async Task ShouldReturnExistingApplication(StrategusBattleMercenaryApplicationStatus existingApplicationStatus)
        {
            Character character = new();
            User user = new() { Characters = { character } };
            ArrangeDb.Users.Add(user);

            StrategusBattle battle = new() { Phase = StrategusBattlePhase.Hiring };
            ArrangeDb.StrategusBattles.Add(battle);

            StrategusBattleMercenaryApplication existingApplication = new()
            {
                Side = StrategusBattleSide.Attacker,
                Status = existingApplicationStatus,
                Battle = battle,
                Character = character,
            };
            ArrangeDb.StrategusBattleMercenaryApplications.Add(existingApplication);
            await ArrangeDb.SaveChangesAsync();

            ApplyAsMercenaryToStrategusBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new()
            {
                UserId = user.Id,
                CharacterId = character.Id,
                BattleId = battle.Id,
                Side = StrategusBattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var application = res.Data!;
            Assert.AreEqual(existingApplication.Id, application.Id);
            Assert.AreEqual(user.Id, application.User.Id);
            Assert.AreEqual(character.Id, application.Character.Id);
            Assert.AreEqual(existingApplicationStatus, application.Status);
        }

        [Test]
        public async Task ShouldApply()
        {
            Character character = new();
            User user = new() { Characters = { character } };
            ArrangeDb.Users.Add(user);

            StrategusBattle battle = new() { Phase = StrategusBattlePhase.Hiring };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            var characterClassModelMock = new Mock<ICharacterClassModel>();
            characterClassModelMock
                .Setup(m => m.ResolveCharacterClass(It.IsAny<CharacterStatistics>()))
                .Returns(CharacterClass.Crossbowman);

            ApplyAsMercenaryToStrategusBattleCommand.Handler handler = new(ActDb, Mapper, characterClassModelMock.Object);
            var res = await handler.Handle(new()
            {
                UserId = user.Id,
                CharacterId = character.Id,
                BattleId = battle.Id,
                Side = StrategusBattleSide.Defender,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var application = res.Data!;
            Assert.NotZero(application.Id);
            Assert.AreEqual(user.Id, application.User.Id);
            Assert.AreEqual(character.Id, application.Character.Id);
            Assert.AreEqual(CharacterClass.Crossbowman, application.Character.Class);
            Assert.AreEqual(StrategusBattleSide.Defender, application.Side);
            Assert.AreEqual(StrategusBattleMercenaryApplicationStatus.Pending, application.Status);
        }
    }
}
