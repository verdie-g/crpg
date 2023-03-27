using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements;

public class AddSettlementItemCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyNotFound()
    {
        AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new AddSettlementItemCommand
        {
            PartyId = 99,
            SettlementId = 99,
            ItemId = "99",
            Count = 0,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotFound));
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

        AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new AddSettlementItemCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            ItemId = "99",
            Count = 0,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotInASettlement));
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

        AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new AddSettlementItemCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            ItemId = "99",
            Count = 0,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotInASettlement));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyGiveItemsTheyDontOwn()
    {
        Item item0 = new() { Id = "0" };
        Item item1 = new() { Id = "1" };
        ArrangeDb.Items.AddRange(item0, item1);

        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.RecruitingInSettlement,
            TargetedSettlement = settlement,
            User = new User(),
            Items = { new PartyItem { Item = item0, Count = 5 } },
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new AddSettlementItemCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            ItemId = item1.Id,
            Count = 0,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotOwned));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyDoesntHaveEnoughItems()
    {
        Item item0 = new() { Id = "0" };
        ArrangeDb.Items.AddRange(item0);

        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.RecruitingInSettlement,
            TargetedSettlement = settlement,
            User = new User(),
            Items = { new PartyItem { Item = item0, Count = 5 } },
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new AddSettlementItemCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            ItemId = item0.Id,
            Count = 6,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotOwned));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyTakesItemsFromUnownedSettlement()
    {
        Item item0 = new() { Id = "0" };
        ArrangeDb.Items.AddRange(item0);

        Settlement settlement = new()
        {
            Items = { new SettlementItem { Item = item0, Count = 2 } },
        };
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.RecruitingInSettlement,
            TargetedSettlement = settlement,
            User = new User(),
            Items = new List<PartyItem>(),
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new AddSettlementItemCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            ItemId = item0.Id,
            Count = -2,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotSettlementOwner));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyTakesItemsTheSettlementDoesNotHave()
    {
        Item item0 = new() { Id = "0" };
        Item item1 = new() { Id = "1" };
        ArrangeDb.Items.AddRange(item0, item1);

        Settlement settlement = new()
        {
            Items = { new SettlementItem { Item = item0, Count = 2 } },
        };
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.RecruitingInSettlement,
            TargetedSettlement = settlement,
            User = new User(),
            Items = new List<PartyItem>(),
            OwnedSettlements = { settlement },
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new AddSettlementItemCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            ItemId = item1.Id,
            Count = -1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotOwned));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyTakesItemsWhenTheSettlementDoesntHaveEnough()
    {
        Item item0 = new() { Id = "0" };
        ArrangeDb.Items.AddRange(item0);

        Settlement settlement = new()
        {
            Items = { new SettlementItem { Item = item0, Count = 2 } },
        };
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.RecruitingInSettlement,
            TargetedSettlement = settlement,
            User = new User(),
            Items = new List<PartyItem>(),
            OwnedSettlements = { settlement },
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new AddSettlementItemCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            ItemId = item0.Id,
            Count = -3,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotOwned));
    }

    [TestCase(3, 7, 11)]
    [TestCase(3, 0, 11)]
    [TestCase(-3, 7, 11)]
    [TestCase(-3, 7, 0)]
    public async Task ShouldGiveTakeItemsToFromSettlement(int diff, int settlementItemCount, int partyItemCount)
    {
        Item item0 = new() { Id = "0" };
        ArrangeDb.Items.AddRange(item0);

        Settlement settlement = new();
        if (settlementItemCount != 0)
        {
            settlement.Items.Add(new SettlementItem { Item = item0, Count = settlementItemCount });
        }

        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            TargetedSettlement = settlement,
            User = new User(),
            OwnedSettlements = { settlement },
        };
        if (partyItemCount != 0)
        {
            party.Items.Add(new PartyItem { Item = item0, Count = partyItemCount });
        }

        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new AddSettlementItemCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            ItemId = item0.Id,
            Count = diff,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var itemStack = res.Data!;
        Assert.That(itemStack.Item.Id, Is.EqualTo(item0.Id));
        Assert.That(itemStack.Count, Is.EqualTo(settlementItemCount + diff));

        settlement = await AssertDb.Settlements
            .Include(s => s.Items)
            .FirstAsync(s => s.Id == settlement.Id);
        Assert.That(settlement.Items.Count, Is.EqualTo(1));
        Assert.That(settlement.Items[0].ItemId, Is.EqualTo(item0.Id));
        Assert.That(settlement.Items[0].Count, Is.EqualTo(settlementItemCount + diff));

        party = await AssertDb.Parties
            .Include(h => h.Items)
            .FirstAsync(h => h.Id == party.Id);
        Assert.That(party.Items.Count, Is.EqualTo(1));
        Assert.That(party.Items[0].ItemId, Is.EqualTo(item0.Id));
        Assert.That(party.Items[0].Count, Is.EqualTo(partyItemCount - diff));
    }
}
