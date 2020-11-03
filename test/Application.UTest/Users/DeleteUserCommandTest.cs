using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Users.Commands;
using Crpg.Domain.Entities;
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
            var user = ArrangeDb.Users.Add(new User
            {
                Characters = new List<Character> { new Character() },
                OwnedItems = new List<UserItem> { new UserItem { Item = new Item() } },
                Bans = new List<Ban> { new Ban() }
            });
            await ArrangeDb.SaveChangesAsync();

            // needs to be saved before UserItems[0] gets deleted
            int itemId = user.Entity.OwnedItems[0].ItemId;

            var handler = new DeleteUserCommand.Handler(ActDb, Mock.Of<IEventService>(), Mock.Of<IDateTimeOffset>());
            await handler.Handle(new DeleteUserCommand
            {
                UserId = user.Entity.Id
            }, CancellationToken.None);

            var dbUser = await AssertDb.Users.FirstOrDefaultAsync(u => u.Id == user.Entity.Id);
            Assert.IsNotNull(dbUser);
            Assert.IsNotNull(dbUser.DeletedAt);

            Assert.ThrowsAsync<InvalidOperationException>(() => AssertDb.Characters.FirstAsync(c => c.Id == user.Entity.Characters[0].Id));
            Assert.ThrowsAsync<InvalidOperationException>(() => AssertDb.UserItems.FirstAsync(oi =>
                oi.UserId == user.Entity.Id && oi.ItemId == user.Entity.OwnedItems[0].ItemId));
            Assert.DoesNotThrowAsync(() => AssertDb.Items.FirstAsync(i => i.Id == itemId));
            Assert.DoesNotThrowAsync(() => AssertDb.Bans.FirstAsync(b => b.BannedUserId == user.Entity.Id));
        }

        [Test]
        public async Task DeleteNonExistingUser()
        {
            var handler = new DeleteUserCommand.Handler(ActDb, Mock.Of<IEventService>(), Mock.Of<IDateTimeOffset>());
            var result = await handler.Handle(new DeleteUserCommand { UserId = 1 }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
        }
    }
}
