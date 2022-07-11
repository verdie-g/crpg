using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Commands;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements;

public class UpdateSettlementCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyIsNotFound()
    {
        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = 1,
            SettlementId = 2,
            Troops = 0,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyNotInASettlement()
    {
        Party party = new() { Status = PartyStatus.Idle, User = new User() };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = 1,
            Troops = 0,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotInASettlement, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyNotInTheSpecifiedSettlement()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            TargetedSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = 99,
            Troops = 0,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotInASettlement, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyIsGivingTroopTheyDontHave()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            Troops = 5,
            TargetedSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Troops = 6,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotEnoughTroops, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyIsTakingTroopsFromANotOwnedSettlement()
    {
        Settlement settlement = new() { Troops = 10 };
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            Troops = 5,
            TargetedSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Troops = 5,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotSettlementOwner, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldGiveTroopsToSettlement()
    {
        Settlement settlement = new()
        {
            Troops = 20,
        };
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.RecruitingInSettlement,
            Troops = 10,
            TargetedSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Troops = 30,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        var settlementVm = res.Data!;
        Assert.AreEqual(settlement.Id, settlementVm.Id);
        Assert.AreEqual(30, AssertDb.Settlements.Find(settlement.Id)!.Troops);
    }

    [Test]
    public async Task ShouldTakeTroopsFromSettlement()
    {
        Settlement settlement = new()
        {
            Troops = 20,
        };
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            Troops = 10,
            TargetedSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        settlement.Owner = party;
        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Troops = 10,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        var settlementVm = res.Data!;
        Assert.AreEqual(settlement.Id, settlementVm.Id);
        Assert.AreEqual(20, AssertDb.Parties.Find(party.Id)!.Troops);
        Assert.AreEqual(10, AssertDb.Settlements.Find(settlement.Id)!.Troops);
    }
}
