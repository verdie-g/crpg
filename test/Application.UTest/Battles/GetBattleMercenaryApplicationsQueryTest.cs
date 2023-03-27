using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class GetBattleMercenaryApplicationsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        GetBattleMercenaryApplicationsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new GetBattleMercenaryApplicationsQuery
        {
            UserId = 99,
            BattleId = 99,
            Statuses = Array.Empty<BattleMercenaryApplicationStatus>(),
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleIsInPreparation()
    {
        Battle battle = new() { Phase = BattlePhase.Preparation };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleMercenaryApplicationsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new GetBattleMercenaryApplicationsQuery
        {
            UserId = 99,
            BattleId = battle.Id,
            Statuses = Array.Empty<BattleMercenaryApplicationStatus>(),
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }

    [Test]
    public async Task ShouldOnlyReturnFighterSideIfUserIsAFighter()
    {
        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
            Fighters = { new BattleFighter { PartyId = 20, Side = BattleSide.Defender } },
            MercenaryApplications =
            {
                new BattleMercenaryApplication
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Attacker,
                    Status = BattleMercenaryApplicationStatus.Pending,
                },
                new BattleMercenaryApplication
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Attacker,
                    Status = BattleMercenaryApplicationStatus.Declined,
                },
                new BattleMercenaryApplication
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Attacker,
                    Status = BattleMercenaryApplicationStatus.Accepted,
                },
                new BattleMercenaryApplication
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Defender,
                    Status = BattleMercenaryApplicationStatus.Pending,
                },
                new BattleMercenaryApplication
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Defender,
                    Status = BattleMercenaryApplicationStatus.Declined,
                },
                new BattleMercenaryApplication
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Defender,
                    Status = BattleMercenaryApplicationStatus.Accepted,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleMercenaryApplicationsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new GetBattleMercenaryApplicationsQuery
        {
            UserId = 20,
            BattleId = battle.Id,
            Statuses = new[]
            {
                BattleMercenaryApplicationStatus.Pending,
                BattleMercenaryApplicationStatus.Declined,
                BattleMercenaryApplicationStatus.Accepted,
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var applications = res.Data!;
        Assert.That(applications.Count, Is.EqualTo(3));
        Assert.That(applications[0].Side, Is.EqualTo(BattleSide.Defender));
    }

    [Test]
    public async Task ShouldOnlyReturnUserApplicationsIfUserIsNotAFighter()
    {
        User user = new();
        ArrangeDb.Users.Add(user);

        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
            Fighters = { new BattleFighter { PartyId = 99, Side = BattleSide.Defender } },
            MercenaryApplications =
            {
                new BattleMercenaryApplication
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Attacker,
                    Status = BattleMercenaryApplicationStatus.Pending,
                },
                new BattleMercenaryApplication
                {
                    Character = new Character { User = user },
                    Side = BattleSide.Attacker,
                    Status = BattleMercenaryApplicationStatus.Pending,
                },
                new BattleMercenaryApplication
                {
                    Character = new Character { User = user },
                    Side = BattleSide.Defender,
                    Status = BattleMercenaryApplicationStatus.Pending,
                },
                new BattleMercenaryApplication
                {
                    Character = new Character { User = user },
                    Side = BattleSide.Defender,
                    Status = BattleMercenaryApplicationStatus.Declined,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleMercenaryApplicationsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new GetBattleMercenaryApplicationsQuery
        {
            UserId = user.Id,
            BattleId = battle.Id,
            Statuses = new[]
            {
                BattleMercenaryApplicationStatus.Pending,
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var applications = res.Data!;
        Assert.That(applications.Count, Is.EqualTo(2));
        Assert.That(applications[0].Side, Is.EqualTo(BattleSide.Attacker));
        Assert.That(applications[1].Side, Is.EqualTo(BattleSide.Defender));
    }
}
