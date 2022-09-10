using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

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
        UserItem userItem1 = new() { BaseItem = new Item { Id = "1" } };
        UserItem userItem2 = new() { BaseItem = new Item { Id = "2" } };
        Character character = new()
        {
            Name = "toto",
            UserId = 2,
            EquippedItems =
            {
                new EquippedItem { UserItem = userItem1, Slot = ItemSlot.Body },
                new EquippedItem { UserItem = userItem2, Slot = ItemSlot.Weapon0 },
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
