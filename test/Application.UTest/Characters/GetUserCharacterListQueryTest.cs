using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Queries;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class GetUserCharacterListQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            ArrangeDb.AddRange(
                new Character
                {
                    Name = "toto",
                    UserId = 1,
                    Statistics = new CharacterStatistics
                    {
                        Attributes = new CharacterAttributes { Points = 1 },
                        Skills = new CharacterSkills { Points = 2 },
                        WeaponProficiencies = new CharacterWeaponProficiencies { Points = 3 },
                    },
                    Items = new CharacterItems { HeadItem = new Item { Name = "4" } },
                },
                new Character
                {
                    Name = "titi",
                    UserId = 1,
                },
                new Character
                {
                    Name = "tata",
                    UserId = 2,
                });
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetUserCharactersListQuery.Handler(ActDb, Mapper);
            var characters = await handler.Handle(new GetUserCharactersListQuery { UserId = 1 }, CancellationToken.None);

            Assert.AreEqual(2, characters.Count);
            Assert.AreEqual(1, characters[0].Statistics.Attributes.Points);
            Assert.AreEqual(2, characters[0].Statistics.Skills.Points);
            Assert.AreEqual(3, characters[0].Statistics.WeaponProficiencies.Points);
            Assert.AreEqual("4", characters[0].Items.HeadItem!.Name);
        }
    }
}