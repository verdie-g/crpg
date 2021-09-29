using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Crpg.Sdk.Abstractions.Events;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users
{
    public class DeleteUserCommandTest : TestBase
    {
        [Test]
        public async Task DeleteExistingUser()
        {
            User user = new()
            {
                Characters = new List<Character> { new() { EquippedItems = new List<EquippedItem> { new() } } },
                Items = new List<UserItem> { new() { Item = new Item() } },
                Bans = new List<Ban> { new() },
                ClanMembership = new ClanMember { Clan = new Clan() },
                Hero = new Hero
                {
                    Items = new List<HeroItem> { new() { Item = new Item() } },
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            // Save ids before they get deleted.
            int itemId = user.Items[0].ItemId;
            int clanId = user.ClanMembership.ClanId;
            int strategusItemId = user.Hero.Items[0].ItemId;

            var userService = Mock.Of<IUserService>();
            DeleteUserCommand.Handler handler = new(ActDb, Mock.Of<IEventService>(), Mock.Of<IDateTimeOffset>(), userService);
            await handler.Handle(new DeleteUserCommand
            {
                UserId = user.Id,
            }, CancellationToken.None);

            var dbUser = await AssertDb.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.IsNotNull(dbUser);
            Assert.IsNotNull(dbUser.DeletedAt);

            Assert.ThrowsAsync<InvalidOperationException>(() => AssertDb.Characters.FirstAsync(c => c.Id == user.Characters[0].Id));
            Assert.ThrowsAsync<InvalidOperationException>(() => AssertDb.UserItems.FirstAsync(oi =>
                oi.UserId == user.Id && oi.ItemId == user.Items[0].ItemId));
            Assert.ThrowsAsync<InvalidOperationException>(() => AssertDb.EquippedItems.FirstAsync(ei =>
                ei.UserId == user.Id));
            Assert.ThrowsAsync<InvalidOperationException>(() => AssertDb.Heroes.FirstAsync(h =>
                h.Id == user.Id));
            Assert.ThrowsAsync<InvalidOperationException>(() => AssertDb.HeroItems.FirstAsync(oi =>
                oi.HeroId == user.Id));
            Assert.DoesNotThrowAsync(() => AssertDb.Items.FirstAsync(i => i.Id == itemId));
            Assert.DoesNotThrowAsync(() => AssertDb.Bans.FirstAsync(b => b.BannedUserId == user.Id));
            Assert.DoesNotThrowAsync(() => AssertDb.Clans.FirstAsync(c => c.Id == clanId));
            Assert.DoesNotThrowAsync(() => AssertDb.Items.FirstAsync(i => i.Id == strategusItemId));
        }

        [Test]
        public async Task DeleteNonExistingUser()
        {
            var userService = Mock.Of<IUserService>();
            DeleteUserCommand.Handler handler = new(ActDb, Mock.Of<IEventService>(), Mock.Of<IDateTimeOffset>(), userService);
            var result = await handler.Handle(new DeleteUserCommand { UserId = 1 }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
        }
    }
}
