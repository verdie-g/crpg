using Crpg.Application.Users.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class UpdateUserDonorsCommandTest : TestBase
{
    [Test]
    public async Task Test()
    {
        User[] users =
        {
            new()
            {
                Platform = Platform.Steam,
                PlatformUserId = "1",
                IsDonor = true,
                Items = Array.Empty<UserItem>(),
            },
            new()
            {
                Platform = Platform.Steam,
                PlatformUserId = "2",
                IsDonor = true,
                Items =
                {
                    new UserItem { BaseItem = new Item { Id = "b", Type = ItemType.Banner } },
                    new UserItem { BaseItem = new Item { Id = "1h", Type = ItemType.OneHandedWeapon } },
                },
            },
            new()
            {
                Platform = Platform.Steam,
                PlatformUserId = "3",
                IsDonor = false,
            },
        };
        ArrangeDb.Users.AddRange(users);
        await ArrangeDb.SaveChangesAsync();

        UpdateUserDonorsCommand.Handler handler = new(ActDb);
        await handler.Handle(new UpdateUserDonorsCommand
        {
            PlatformUserIds = new[] { "1", "3" },
        }, CancellationToken.None);

        var usersDb = await AssertDb.Users.Include(u => u.Items).ToArrayAsync();
        var user1 = usersDb.First(u => u.PlatformUserId == "1");
        Assert.IsTrue(user1.IsDonor);
        Assert.AreEqual(0, user1.Items.Count);

        var user2 = usersDb.First(u => u.PlatformUserId == "2");
        Assert.IsFalse(user2.IsDonor);
        Assert.AreEqual(1, user2.Items.Count);

        var user3 = usersDb.First(u => u.PlatformUserId == "3");
        Assert.IsTrue(user3.IsDonor);
        Assert.AreEqual(0, user3.Items.Count);
    }
}
