using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class StrategusSpeedModelTest
{
    [Test]
    public void TroopsShouldUseTheBestMountTheyHave()
    {
        Party party1 = new()
        {
            Troops = 10,
            Items = new List<PartyItem>
            {
                PartyItemMount(450, 5),
                PartyItemMount(350, 5),
                PartyItemMount(250, 5),
            },
        };
        Party party2 = new()
        {
            Troops = 10,
            Items = new List<PartyItem>
            {
                PartyItemMount(450, 5),
                PartyItemMount(350, 10),
                PartyItemMount(250, 10),
            },
        };
        StrategusSpeedModel speedModel = new();
        Assert.That(speedModel.ComputePartySpeed(party1), Is.GreaterThanOrEqualTo(speedModel.ComputePartySpeed(party2)));
    }

    [Test]
    public void RecruitingTroopShouldDecreaseSpeed()
    {
        int fastHorseCount = 150;
        int mediumSpeedHorseCount = 100;
        int slowHorseCount = 50;
        int totalHorseCount = fastHorseCount + mediumSpeedHorseCount + slowHorseCount;
        double previousSpeed = double.MaxValue;
        StrategusSpeedModel speedModel = new();
        for (int troops = 10; troops <= 1000; troops += 10)
        {
            Party party = new()
            {
                Troops = troops,
                Items = new List<PartyItem>
                {
                    PartyItemMount(450, fastHorseCount),
                    PartyItemMount(350, mediumSpeedHorseCount),
                    PartyItemMount(250, slowHorseCount),
                },
            };
            double speed = speedModel.ComputePartySpeed(party);
            if (troops < totalHorseCount)
            {
                /*
                this is in case there is enough mount for everyone soldier to be mounted.
                The soldier will choose by default the fastest mounts they can find
                In this case the speed of the army is the speed of the slowest mount among the used one
                (which is worst of the top tier mounts) .
                In this case the speed may not increase but should not decrease
                */
                Assert.That(speed, Is.LessThanOrEqualTo(previousSpeed));
            }
            else
            {
                /*
                This is in case there is not enough mounts for every soldier to be mounted the model for this is
                assuming some of the soldiers have to walk. The more of them walk , the slowest the party get.
                The speed should strictly decrease.
                */
                Assert.Less(speed, previousSpeed);
            }

            previousSpeed = speed;
        }
    }

    [Test]
    public void BuyingMountsShouldIncreaseSpeed()
    {
        double previousSpeed = 0;
        StrategusSpeedModel speedModel = new();
        for (int mountCountFactor = 1; mountCountFactor <= 100; mountCountFactor++)
        {
            Party party = new()
            {
                Troops = 1000,
                Items = new List<PartyItem>
                {
                    PartyItemMount(450, 6 * mountCountFactor),
                    PartyItemMount(350, 2 * mountCountFactor),
                    PartyItemMount(250, 2 * mountCountFactor),
                },
            };
            double speed = speedModel.ComputePartySpeed(party);
            Assert.That(speed, Is.GreaterThan(previousSpeed));
            previousSpeed = speed;
        }
    }

    private PartyItem PartyItemMount(int hitPoints, int count)
    {
        return new()
        {
            Item = new Item { Mount = new ItemMountComponent { HitPoints = hitPoints } },
            Count = count,
        };
    }
}
