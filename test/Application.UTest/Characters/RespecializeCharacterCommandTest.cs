using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class RespecializeCharacterCommandTest : TestBase
    {
        private static readonly Constants Constants = new()
        {
            RespecializeExperiencePenaltyCoefs = new[] { 0.5f, 0f },
        };

        [Test]
        public async Task RespecializeCharacterLevel3ShouldMakeItLevel2()
        {
            var character = new Character
            {
                Generation = 2,
                Level = 3,
                Experience = 150,
                ExperienceMultiplier = 1.1f,
                EquippedItems =
                {
                    new EquippedItem { Item = new Item(), Slot = ItemSlot.Head },
                    new EquippedItem { Item = new Item(), Slot = ItemSlot.Body },
                    new EquippedItem { Item = new Item(), Slot = ItemSlot.Weapon0 },
                },
            };
            ArrangeDb.Add(character);
            await ArrangeDb.SaveChangesAsync();

            var experienceTableMock = new Mock<IExperienceTable>();
            experienceTableMock.Setup(et => et.GetLevelForExperience(75)).Returns(2);

            var characterServiceMock = new Mock<ICharacterService>();

            RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object, experienceTableMock.Object, Constants);
            await handler.Handle(new RespecializeCharacterCommand
            {
                CharacterId = character.Id,
                UserId = character.UserId,
            }, CancellationToken.None);

            character = await AssertDb.Characters
                .Include(c => c.EquippedItems)
                .FirstAsync(c => c.Id == character.Id);
            Assert.AreEqual(2, character.Generation);
            Assert.AreEqual(2, character.Level);
            Assert.AreEqual(75, character.Experience);
            Assert.AreEqual(1.1f, character.ExperienceMultiplier);
            Assert.IsEmpty(character.EquippedItems);
            characterServiceMock.Verify(cs => cs.ResetCharacterStats(It.IsAny<Character>(), true));
        }

        [Test]
        public async Task ShouldReturnNotFoundIfUserDoesntExist()
        {
            var experienceTable = Mock.Of<IExperienceTable>();
            var characterService = Mock.Of<ICharacterService>();
            RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterService, experienceTable, Constants);
            var result = await handler.Handle(
                new RespecializeCharacterCommand
                {
                    CharacterId = 1,
                    UserId = 2,
                }, CancellationToken.None);

            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnNotFoundIfCharacterDoesntExist()
        {
            var user = ArrangeDb.Users.Add(new User());
            await ArrangeDb.SaveChangesAsync();

            var experienceTable = Mock.Of<IExperienceTable>();
            var characterService = Mock.Of<ICharacterService>();
            RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterService, experienceTable, Constants);
            var result = await handler.Handle(
                new RespecializeCharacterCommand
                {
                    CharacterId = 1,
                    UserId = user.Entity.Id,
                }, CancellationToken.None);

            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }
    }
}
