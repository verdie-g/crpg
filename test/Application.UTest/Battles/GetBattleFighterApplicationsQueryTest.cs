using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class GetBattleFighterApplicationsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        GetBattleFighterApplicationsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleFighterApplicationsQuery
        {
            PartyId = 99,
            BattleId = 99,
            Statuses = Array.Empty<BattleFighterApplicationStatus>(),
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.BattleNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldOnlyReturnFighterSideIfUserIsACommander()
    {
        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
            Fighters = { new BattleFighter { PartyId = 20, Side = BattleSide.Defender, Commander = true } },
            FighterApplications =
            {
                new BattleFighterApplication
                {
                    Party = new Party { User = new User() },
                    Side = BattleSide.Attacker,
                    Status = BattleFighterApplicationStatus.Pending,
                },
                new BattleFighterApplication
                {
                    Party = new Party { User = new User() },
                    Side = BattleSide.Attacker,
                    Status = BattleFighterApplicationStatus.Declined,
                },
                new BattleFighterApplication
                {
                    Party = new Party { User = new User() },
                    Side = BattleSide.Attacker,
                    Status = BattleFighterApplicationStatus.Accepted,
                },
                new BattleFighterApplication
                {
                    Party = new Party { User = new User() },
                    Side = BattleSide.Defender,
                    Status = BattleFighterApplicationStatus.Pending,
                },
                new BattleFighterApplication
                {
                    Party = new Party { User = new User() },
                    Side = BattleSide.Defender,
                    Status = BattleFighterApplicationStatus.Declined,
                },
                new BattleFighterApplication
                {
                    Party = new Party { User = new User() },
                    Side = BattleSide.Defender,
                    Status = BattleFighterApplicationStatus.Accepted,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleFighterApplicationsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleFighterApplicationsQuery
        {
            PartyId = 20,
            BattleId = battle.Id,
            Statuses = new[]
            {
                BattleFighterApplicationStatus.Pending,
                BattleFighterApplicationStatus.Declined,
                BattleFighterApplicationStatus.Accepted,
            },
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        var applications = res.Data!;
        Assert.AreEqual(3, applications.Count);
        Assert.AreEqual(BattleSide.Defender, applications[0].Side);
    }

    [Theory]
    public async Task ShouldOnlyReturnPartyApplicationsIfPartyIsNotACommanderOrFighter(bool isFighter)
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);

        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
            Fighters = { new BattleFighter { PartyId = 99, Side = BattleSide.Defender } },
            FighterApplications =
            {
                new BattleFighterApplication
                {
                    Party = new Party { User = new User() },
                    Side = BattleSide.Attacker,
                    Status = BattleFighterApplicationStatus.Pending,
                },
                new BattleFighterApplication
                {
                    Party = party,
                    Side = BattleSide.Attacker,
                    Status = BattleFighterApplicationStatus.Pending,
                },
                new BattleFighterApplication
                {
                    Party = party,
                    Side = BattleSide.Defender,
                    Status = BattleFighterApplicationStatus.Pending,
                },
                new BattleFighterApplication
                {
                    Party = party,
                    Side = BattleSide.Defender,
                    Status = BattleFighterApplicationStatus.Declined,
                },
            },
        };
        if (isFighter)
        {
            battle.Fighters.Add(new BattleFighter
            {
                Party = party,
                Side = BattleSide.Defender,
                Commander = false,
            });
        }

        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleFighterApplicationsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleFighterApplicationsQuery
        {
            PartyId = party.Id,
            BattleId = battle.Id,
            Statuses = new[]
            {
                BattleFighterApplicationStatus.Pending,
            },
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        var applications = res.Data!;
        Assert.AreEqual(2, applications.Count);
        Assert.AreEqual(BattleSide.Attacker, applications[0].Side);
        Assert.AreEqual(BattleSide.Defender, applications[1].Side);
    }
}
