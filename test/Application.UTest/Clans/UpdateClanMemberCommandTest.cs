using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Clans.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans
{
    public class UpdateClanMemberCommandTest : TestBase
    {
        private static readonly IClanService ClanService = new ClanService();

        [TestCase(ClanMemberRole.Member)]
        [TestCase(ClanMemberRole.Officer)]
        public async Task ShouldReturnErrorIfUserNotLeader(ClanMemberRole userRole)
        {
            Clan clan = new();
            User user = new() { ClanMembership = new ClanMember { Clan = clan, Role = userRole } };
            User member = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
            ArrangeDb.Users.AddRange(user, member);
            await ArrangeDb.SaveChangesAsync();

            var res = await new UpdateClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                MemberId = member.Id,
                Role = ClanMemberRole.Officer,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ClanMemberRoleNotMet, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfLeaderChangesItsRole()
        {
            Clan clan = new();
            User user = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Leader } };
            ArrangeDb.Users.AddRange(user);
            await ArrangeDb.SaveChangesAsync();

            var res = await new UpdateClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                MemberId = user.Id,
                Role = ClanMemberRole.Officer,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ClanMemberRoleNotMet, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldUpdateMember()
        {
            Clan clan = new();
            User user = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Leader } };
            User member = new() { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
            ArrangeDb.Users.AddRange(user, member);
            await ArrangeDb.SaveChangesAsync();

            var res = await new UpdateClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                MemberId = member.Id,
                Role = ClanMemberRole.Officer,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var memberVm = res.Data!;
            Assert.AreEqual(member.Id, memberVm.User.Id);
            Assert.AreEqual(ClanMemberRole.Officer, memberVm.Role);
        }
    }
}
