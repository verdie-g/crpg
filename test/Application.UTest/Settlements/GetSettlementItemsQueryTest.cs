using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Queries;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements;

public class GetSettlementItemsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyNotFound()
    {
        GetSettlementItemsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetSettlementItemsQuery
        {
            PartyId = 99,
            SettlementId = 99,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyNotInASettlement()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);
        Party party = new()
        {
            Status = PartyStatus.Idle,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        GetSettlementItemsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetSettlementItemsQuery
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotInASettlement, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyNotInTheSettlement()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);
        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            TargetedSettlement = new Settlement(),
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        GetSettlementItemsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetSettlementItemsQuery
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotInASettlement, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyIsNotSettlementOwner()
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

        GetSettlementItemsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetSettlementItemsQuery
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotSettlementOwner, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnSettlementItems()
    {
        Settlement settlement = new()
        {
            Items =
            {
                new SettlementItem { Item = new Item() },
                new SettlementItem { Item = new Item() },
            },
        };
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            TargetedSettlement = settlement,
            User = new User(),
            OwnedSettlements = { settlement },
        };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        GetSettlementItemsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetSettlementItemsQuery
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        Assert.AreEqual(2, res.Data!.Count);
    }
}
