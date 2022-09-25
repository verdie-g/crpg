using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class SellUserItemCommandTest : TestBase
{
    [Test]
    public async Task ShouldCallItemService()
    {
        User user = new()
        {
            Gold = 0,
            Items = new List<UserItem>
            {
                new()
                {
                    BaseItem = new Item { Price = 100 },
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IItemService> itemServiceMock = new();

        await new SellUserItemCommand.Handler(ActDb, itemServiceMock.Object).Handle(new SellUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);

        itemServiceMock.Verify(s => s.SellUserItem(ActDb, It.IsAny<UserItem>()));
    }

    [Test]
    public async Task NotFoundItem()
    {
        var user = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var itemService = Mock.Of<IItemService>();
        var result = await new SellUserItemCommand.Handler(ActDb, itemService).Handle(
            new SellUserItemCommand
            {
                UserItemId = 1,
                UserId = user.Entity.Id,
            }, CancellationToken.None);
        Assert.AreEqual(ErrorCode.UserItemNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task NotFoundUserItem()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var itemService = Mock.Of<IItemService>();
        var result = await new SellUserItemCommand.Handler(ActDb, itemService).Handle(
            new SellUserItemCommand
            {
                UserItemId = 1,
                UserId = user.Id,
            }, CancellationToken.None);
        Assert.AreEqual(ErrorCode.UserItemNotFound, result.Errors![0].Code);
    }
}
