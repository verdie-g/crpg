using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
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
                    EquippedItems = { new EquippedItem { Item = new Item { Name = "4" }, Slot = ItemSlot.Head } },
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
            var result = await handler.Handle(new GetUserCharactersListQuery { UserId = 1 }, CancellationToken.None);

            var characters = result.Data!;
            Assert.AreEqual(2, characters.Count);
            Assert.AreEqual(1, characters[0].Statistics.Attributes.Points);
            Assert.AreEqual(2, characters[0].Statistics.Skills.Points);
            Assert.AreEqual(3, characters[0].Statistics.WeaponProficiencies.Points);
            Assert.AreEqual("4", characters[0].EquippedItems[0].Item.Name);
            Assert.AreEqual(ItemSlot.Head, characters[0].EquippedItems[0].Slot);
        }
    }
}
