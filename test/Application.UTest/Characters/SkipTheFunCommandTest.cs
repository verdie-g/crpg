using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class SkipTheFunCommandTest : TestBase
    {
        [Test]
        public async Task ShouldSkipTheFunCorrectly()
        {
            var experienceTableMock = new Mock<IExperienceTable>();
            experienceTableMock.Setup(et => et.GetExperienceForLevel(30)).Returns(30000);

            var characterServiceMock = new Mock<ICharacterService>();

            var character = new Character { Generation = 2, Level = 3, Experience = 250 };
            ArrangeDb.Characters.Add(character);
            await ArrangeDb.SaveChangesAsync();

            var handler = new SkipTheFunCommand.Handler(ActDb, characterServiceMock.Object, experienceTableMock.Object);
            var result = await handler.Handle(new SkipTheFunCommand
            {
                CharacterId = character.Id,
                UserId = character.UserId,
            }, CancellationToken.None);

            Assert.IsNull(result.Errors);
            character = AssertDb.Characters.First(c => c.Id == character.Id);
            Assert.AreEqual(2, character.Generation);
            Assert.AreEqual(30, character.Level);
            Assert.AreEqual(30000, character.Experience);
            Assert.IsTrue(character.SkippedTheFun);

            characterServiceMock.Verify(cs => cs.ResetCharacterStats(It.IsAny<Character>(), true), Times.Once);
        }

        [Test]
        public async Task ShouldReturnNotFoundIfCharacterNotFound()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new SkipTheFunCommand.Handler(ActDb, Mock.Of<ICharacterService>(),
                Mock.Of<IExperienceTable>());
            var result = await handler.Handle(new SkipTheFunCommand
            {
                UserId = user.Id,
                CharacterId = 1,
            }, CancellationToken.None);

            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }
    }
}
