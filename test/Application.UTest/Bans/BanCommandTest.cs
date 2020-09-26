using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Bans.Commands;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Bans
{
    public class BanCommandTest : TestBase
    {
        [Test]
        public async Task BanExistingUser()
        {
            var user1 = ArrangeDb.Users.Add(new User());
            var user2 = ArrangeDb.Users.Add(new User { PlatformId = "1234", Name = "toto" });
            await ArrangeDb.SaveChangesAsync();

            var ban = await new BanCommand.Handler(ActDb, Mapper).Handle(new BanCommand
            {
                BannedUserId = user1.Entity.Id,
                Duration = TimeSpan.FromDays(1),
                Reason = "toto",
                BannedByUserId = user2.Entity.Id,
            }, CancellationToken.None);

            Assert.AreEqual(user1.Entity.Id, ban.BannedUser!.Id);
            Assert.AreEqual(TimeSpan.FromDays(1), ban.Duration);
            Assert.AreEqual("toto", ban.Reason);
            Assert.AreEqual(user2.Entity.Id, ban.BannedByUser!.Id);
            Assert.AreEqual(user2.Entity.PlatformId, ban.BannedByUser.PlatformId);
            Assert.AreEqual(user2.Entity.Name, ban.BannedByUser.Name);
        }

        [Test]
        public async Task BanNonExistingUserShouldThrowNotFound()
        {
            var user2 = ArrangeDb.Users.Add(new User());
            await ArrangeDb.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new BanCommand.Handler(ActDb, Mapper).Handle(new BanCommand
            {
                BannedUserId = 10,
                Duration = TimeSpan.FromDays(1),
                Reason = "toto",
                BannedByUserId = user2.Entity.Id,
            }, CancellationToken.None));
        }

        [Test]
        public async Task BanByNonExistingUserShouldThrowNotFound()
        {
            var user1 = ArrangeDb.Users.Add(new User());
            await ArrangeDb.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new BanCommand.Handler(ActDb, Mapper).Handle(new BanCommand
            {
                BannedUserId = user1.Entity.Id,
                Duration = TimeSpan.FromDays(1),
                Reason = "toto",
                BannedByUserId = 10,
            }, CancellationToken.None));
        }

        [Test]
        public void BanningYourselfShouldReturnError()
        {
            var res = new BanCommand.Validator().Validate(new BanCommand
            {
                BannedUserId = 1,
                Duration = TimeSpan.Zero,
                Reason = "aaa",
                BannedByUserId = 1,
            });

            Assert.AreEqual(1, res.Errors.Count);
        }

        [Test]
        public void EmptyBanReasonShouldTrow()
        {
            var res = new BanCommand.Validator().Validate(new BanCommand
            {
                BannedUserId = 1,
                Duration = TimeSpan.Zero,
                Reason = "",
                BannedByUserId = 2,
            });

            Assert.AreEqual(1, res.Errors.Count);
        }
    }
}