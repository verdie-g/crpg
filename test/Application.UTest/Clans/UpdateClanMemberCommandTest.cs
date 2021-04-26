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
        [TestCase(ClanMemberRole.Admin)]
        public async Task ShouldReturnErrorIfUserNotLeader(ClanMemberRole userRole)
        {
            var clan = new Clan();
            var user = new User { ClanMembership = new ClanMember { Clan = clan, Role = userRole } };
            var member = new User { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
            ArrangeDb.Users.AddRange(user, member);
            await ArrangeDb.SaveChangesAsync();

            var res = await new UpdateClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                MemberId = member.Id,
                Role = ClanMemberRole.Admin,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ClanMemberRoleNotMet, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldUpdateMember()
        {
            var clan = new Clan();
            var user = new User { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Leader } };
            var member = new User { ClanMembership = new ClanMember { Clan = clan, Role = ClanMemberRole.Member } };
            ArrangeDb.Users.AddRange(user, member);
            await ArrangeDb.SaveChangesAsync();

            var res = await new UpdateClanMemberCommand.Handler(ActDb, Mapper, ClanService).Handle(new UpdateClanMemberCommand
            {
                UserId = user.Id,
                ClanId = clan.Id,
                MemberId = member.Id,
                Role = ClanMemberRole.Admin,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var memberVm = res.Data!;
            Assert.AreEqual(member.Id, memberVm.User.Id);
            Assert.AreEqual(ClanMemberRole.Admin, memberVm.Role);
        }
    }
}
