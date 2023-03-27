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
        Assert.That(user1.IsDonor, Is.True);
        Assert.That(user1.Items.Count, Is.EqualTo(0));

        var user2 = usersDb.First(u => u.PlatformUserId == "2");
        Assert.That(user2.IsDonor, Is.False);
        Assert.That(user2.Items.Count, Is.EqualTo(1));

        var user3 = usersDb.First(u => u.PlatformUserId == "3");
        Assert.That(user3.IsDonor, Is.True);
        Assert.That(user3.Items.Count, Is.EqualTo(0));
    }
}
