using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class GetUserCharacterItemsQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfCharacterDoesntExist()
        {
            GetUserCharacterItemsQuery.Handler handler = new(ActDb, Mapper);
            var result = await handler.Handle(new GetUserCharacterItemsQuery
            {
                CharacterId = 1,
                UserId = 2,
            }, CancellationToken.None);

            Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnCharacterItems()
        {
            Character character = new()
            {
                Name = "toto",
                UserId = 2,
                EquippedItems =
                {
                    new EquippedItem { Item = new Item(), Slot = ItemSlot.Body },
                    new EquippedItem { Item = new Item(), Slot = ItemSlot.Weapon0 },
                },
            };
            ArrangeDb.Characters.Add(character);
            await ArrangeDb.SaveChangesAsync();

            GetUserCharacterItemsQuery.Handler handler = new(ActDb, Mapper);
            var result = await handler.Handle(new GetUserCharacterItemsQuery
            {
                CharacterId = character.Id,
                UserId = 2,
            }, CancellationToken.None);

            Assert.IsNull(result.Errors);
            Assert.AreEqual(2, result.Data!.Count);
        }
    }
}
