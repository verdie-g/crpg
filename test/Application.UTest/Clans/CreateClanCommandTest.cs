using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Clans.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans
{
    public class CreateClanCommandTest : TestBase
    {
        [Test]
        public async Task ShouldCreateClan()
        {
            var user = new User();
            ArrangeDb.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var result = await new CreateClanCommand.Handler(ActDb, Mapper).Handle(new CreateClanCommand
            {
                UserId = user.Id,
                Tag = "TW",
                Color = "#12abd3",
                Name = "TaleWorlds",
            }, CancellationToken.None);

            var clan = result.Data!;
            Assert.AreEqual("TW", clan.Tag);
            Assert.AreEqual("#12ABD3", clan.Color);
            Assert.AreEqual("TaleWorlds", clan.Name);

            Assert.That(AssertDb.Clans, Has.Exactly(1).Matches<Clan>(c => c.Id == clan.Id));
            Assert.That(AssertDb.ClanMembers, Has.Exactly(1)
                .Matches<ClanMember>(cm => cm.ClanId == clan.Id && cm.UserId == user.Id));
        }

        [Test]
        public async Task ShouldReturnErrorIfUserDoesntExist()
        {
            var result = await new CreateClanCommand.Handler(ActDb, Mapper).Handle(new CreateClanCommand
            {
                UserId = 1,
                Tag = "TW",
                Color = "#12abd3",
                Name = "TaleWorlds",
            }, CancellationToken.None);

            Assert.NotNull(result.Errors);
            Assert.IsNotEmpty(result.Errors!);
            Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfUserIsAlreadyInAClan()
        {
            var user = new User { ClanMembership = new ClanMember { Clan = new Clan() } };
            ArrangeDb.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var result = await new CreateClanCommand.Handler(ActDb, Mapper).Handle(new CreateClanCommand
            {
                UserId = user.Id,
                Tag = "TW",
                Color = "#12abd3",
                Name = "TaleWorlds",
            }, CancellationToken.None);

            Assert.NotNull(result.Errors);
            Assert.IsNotEmpty(result.Errors!);
            Assert.AreEqual(ErrorCode.UserAlreadyInAClan, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfTagAlreadyExists()
        {
            var user = new User();
            ArrangeDb.Add(user);
            ArrangeDb.Clans.Add(new Clan { Tag = "TW" });
            await ArrangeDb.SaveChangesAsync();

            var result = await new CreateClanCommand.Handler(ActDb, Mapper).Handle(new CreateClanCommand
            {
                UserId = user.Id,
                Tag = "TW",
                Color = "#12abd3",
                Name = "TaleWorlds",
            }, CancellationToken.None);

            Assert.NotNull(result.Errors);
            Assert.IsNotEmpty(result.Errors!);
            Assert.AreEqual(ErrorCode.ClanTagAlreadyUsed, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfNameAlreadyExists()
        {
            var user = new User();
            ArrangeDb.Add(user);
            ArrangeDb.Clans.Add(new Clan { Name = "TaleWorlds" });
            await ArrangeDb.SaveChangesAsync();

            var result = await new CreateClanCommand.Handler(ActDb, Mapper).Handle(new CreateClanCommand
            {
                UserId = user.Id,
                Tag = "TW",
                Color = "#12abd3",
                Name = "TaleWorlds",
            }, CancellationToken.None);

            Assert.NotNull(result.Errors);
            Assert.IsNotEmpty(result.Errors!);
            Assert.AreEqual(ErrorCode.ClanNameAlreadyUsed, result.Errors![0].Code);
        }
    }
}
