using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Commands;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class DeleteUserCommandTest : TestBase
{
    [Test]
    public async Task DeleteExistingUser()
    {
        User user = new()
        {
            Characters = new List<Character> { new() { EquippedItems = new List<EquippedItem> { new() } } },
            Items = new List<UserItem> { new() { Item = new Item { Id = "1" } } },
            Restrictions = new List<Restriction> { new() },
            ClanMembership = new ClanMember { Clan = new Clan() },
            Party = new Party
            {
                Items = new List<PartyItem> { new() { Item = new Item() } },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        // Save ids before they get deleted.
        string itemId = user.Items[0].ItemId;
        int clanId = user.ClanMembership.ClanId;
        string partyItemId = user.Party.Items[0].ItemId;

        var userService = Mock.Of<IUserService>();
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };
        DeleteUserCommand.Handler handler = new(ActDb, Mock.Of<IDateTime>(), userService, activityLogServiceMock.Object);
        await handler.Handle(new DeleteUserCommand
        {
            UserId = user.Id,
        }, CancellationToken.None);

        var dbUser = await AssertDb.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser!.DeletedAt, Is.Not.Null);

        Assert.That(() => AssertDb.Characters.FirstAsync(c => c.Id == user.Characters[0].Id),
            Throws.InstanceOf<InvalidOperationException>());
        Assert.That(() => AssertDb.UserItems.FirstAsync(ui => ui.UserId == user.Id && ui.Id == user.Items[0].Id),
            Throws.InstanceOf<InvalidOperationException>());
        Assert.That(() => AssertDb.EquippedItems.FirstAsync(ei => ei.UserItem!.UserId == user.Id),
            Throws.InstanceOf<InvalidOperationException>());
        Assert.That(() => AssertDb.Parties.FirstAsync(h => h.Id == user.Id),
            Throws.InstanceOf<InvalidOperationException>());
        Assert.That(() => AssertDb.PartyItems.FirstAsync(oi => oi.PartyId == user.Id), Throws.InstanceOf<InvalidOperationException>());
        Assert.That(() => AssertDb.Items.FirstAsync(i => i.Id == itemId), Throws.Nothing);
        Assert.That(() => AssertDb.Restrictions.FirstAsync(r => r.RestrictedUserId == user.Id), Throws.Nothing);
        Assert.That(() => AssertDb.Clans.FirstAsync(c => c.Id == clanId), Throws.Nothing);
        Assert.That(() => AssertDb.Items.FirstAsync(i => i.Id == partyItemId), Throws.Nothing);
    }

    [Test]
    public async Task DeleteNonExistingUser()
    {
        var userService = Mock.Of<IUserService>();
        DeleteUserCommand.Handler handler = new(ActDb, Mock.Of<IDateTime>(), userService, Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new DeleteUserCommand { UserId = 1 }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }
}
