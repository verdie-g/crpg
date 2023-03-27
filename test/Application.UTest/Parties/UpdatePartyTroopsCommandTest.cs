using Crpg.Application.Common;
using Crpg.Application.Parties.Commands;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Parties;

public class UpdatePartyTroopsCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        StrategusTroopRecruitmentPerHour = 5,
        StrategusMaxPartyTroops = 10,
    };

    [Test]
    public async Task ShouldIncreaseTroopsOfPartiesRecruiting()
    {
        Party party1 = new() { Troops = 0, Status = PartyStatus.RecruitingInSettlement, User = new User() };
        Party party2 = new() { Troops = 5, Status = PartyStatus.RecruitingInSettlement, User = new User() };
        Party party3 = new() { Troops = 9, Status = PartyStatus.RecruitingInSettlement, User = new User() };
        Party party4 = new() { Troops = 2, Status = PartyStatus.IdleInSettlement, User = new User() };
        ArrangeDb.Parties.AddRange(party1, party2, party3, party4);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyTroopsCommand.Handler handler = new(ActDb, Constants);
        await handler.Handle(new UpdatePartyTroopsCommand
        {
            DeltaTime = TimeSpan.FromHours(1),
        }, CancellationToken.None);

        party1 = await AssertDb.Parties.FirstAsync(h => h.Id == party1.Id);
        Assert.That(party1.Troops, Is.EqualTo(5));
        party2 = await AssertDb.Parties.FirstAsync(h => h.Id == party2.Id);
        Assert.That(party2.Troops, Is.EqualTo(10));
        party3 = await AssertDb.Parties.FirstAsync(h => h.Id == party3.Id);
        Assert.That(party3.Troops, Is.EqualTo(10));
        party4 = await AssertDb.Parties.FirstAsync(h => h.Id == party4.Id);
        Assert.That(party4.Troops, Is.EqualTo(2));
    }
}
