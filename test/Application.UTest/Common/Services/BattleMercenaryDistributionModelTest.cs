using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class BattleMercenaryDistributionModelTest
{
    private static readonly BattleMercenaryUniformDistributionModel Distribution = new();

    [Test]
    public void ShouldDistributeUniformly()
    {
        BattleFighter[] fighters =
        {
            NewFighter(100, BattleSide.Attacker),
            NewFighter(200, BattleSide.Attacker),
            NewFighter(300, BattleSide.Attacker),
            NewFighter(500, BattleSide.Defender),
            NewFighter(500, BattleSide.Defender),
        };

        int battleSlots = 100;
        Distribution.DistributeMercenaries(fighters, battleSlots);

        Assert.AreEqual(16, fighters[0].MercenarySlots);
        Assert.AreEqual(32, fighters[1].MercenarySlots);
        Assert.AreEqual(49, fighters[2].MercenarySlots);
        Assert.AreEqual(battleSlots - 3, fighters.Take(3).Sum(f => f.MercenarySlots));

        Assert.AreEqual(49, fighters[3].MercenarySlots);
        Assert.AreEqual(49, fighters[4].MercenarySlots);
        Assert.AreEqual(battleSlots - 2, fighters.Skip(3).Sum(f => f.MercenarySlots));
    }

    [Test]
    public void ShouldIgnoreDecimalsOfHeroTroops()
    {
        BattleFighter[] fighters =
        {
            NewFighter(2.9f, BattleSide.Attacker),
            NewFighter(2.8f, BattleSide.Attacker),
            NewFighter(2.7f, BattleSide.Attacker),
        };

        Distribution.DistributeMercenaries(fighters, 6);
        Assert.AreEqual(1, fighters[0].MercenarySlots);
        Assert.AreEqual(1, fighters[1].MercenarySlots);
        Assert.AreEqual(1, fighters[2].MercenarySlots);
    }

    [Test]
    public void ShouldGiveRemainingOfTheSlotsDivisionToTheFirstFighters()
    {
        BattleFighter[] fighters =
        {
            NewFighter(2f, BattleSide.Attacker),
            NewFighter(2f, BattleSide.Attacker),
            NewFighter(2f, BattleSide.Attacker),
        };

        Distribution.DistributeMercenaries(fighters, 8);
        Assert.AreEqual(2, fighters[0].MercenarySlots);
        Assert.AreEqual(2, fighters[1].MercenarySlots);
        Assert.AreEqual(1, fighters[2].MercenarySlots);
    }

    private BattleFighter NewFighter(float troops, BattleSide side) => new()
    {
        Hero = new Hero { Troops = troops, User = new User() },
        Side = side,
    };
}
