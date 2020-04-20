using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Users.Commands;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users
{
    public class DeleteUserCommandTest : TestBase
    {
        [Test]
        public async Task DeleteExistingUser()
        {
            var user = Db.Users.Add(new User
            {
                Characters = new List<Character> { new Character() },
                UserItems = new List<UserItem> { new UserItem { Item = new Item() } },
                Bans = new List<Ban> { new Ban() }
            });
            await Db.SaveChangesAsync();

            await new DeleteUserCommand.Handler(Db).Handle(new DeleteUserCommand
            {
                UserId = user.Entity.Id
            }, CancellationToken.None);

            Assert.ThrowsAsync<InvalidOperationException>(() => Db.Users.FirstAsync(u => u.Id == user.Entity.Id));
            Assert.ThrowsAsync<InvalidOperationException>(() => Db.Characters.FirstAsync(c => c.Id == user.Entity.Characters[0].Id));
            Assert.ThrowsAsync<InvalidOperationException>(() => Db.Bans.FirstAsync(b =>
                b.BannedUserId == user.Entity.Id || b.BannedByUserId == user.Entity.Id));
            Assert.ThrowsAsync<InvalidOperationException>(() => Db.UserItems.FirstAsync(ui =>
                ui.UserId == user.Entity.Id && ui.ItemId == user.Entity.UserItems[0].ItemId));
            Assert.DoesNotThrowAsync(() => Db.Items.FirstAsync(i => i.Id == user.Entity.UserItems[0].ItemId));
        }

        [Test]
        public void DeleteNonExistingUser()
        {
            Assert.ThrowsAsync<NotFoundException>(() =>
                new DeleteUserCommand.Handler(Db).Handle(new DeleteUserCommand { UserId = 1 }, CancellationToken.None));
        }
    }
}