using System.Collections.Generic;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Items;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services
{
    public class StrategusSpeedModelTest
    {
        [Test]
        public void TroopsShouldUseTheBestMountTheyHave()
        {
            Hero hero1 = new()
            {
                Troops = 10,
                Items = new List<HeroItem>
                {
                    HeroItemMount(450, 5),
                    HeroItemMount(350, 5),
                    HeroItemMount(250, 5),
                },
            };
            Hero hero2 = new()
            {
                Troops = 10,
                Items = new List<HeroItem>
                {
                    HeroItemMount(450, 5),
                    HeroItemMount(350, 10),
                    HeroItemMount(250, 10),
                },
            };
            StrategusSpeedModel speedModel = new();
            Assert.GreaterOrEqual(speedModel.ComputeHeroSpeed(hero1), speedModel.ComputeHeroSpeed(hero2));
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
                Hero hero = new()
                {
                    Troops = troops,
                    Items = new List<HeroItem>
                    {
                        HeroItemMount(450, fastHorseCount),
                        HeroItemMount(350, mediumSpeedHorseCount),
                        HeroItemMount(250, slowHorseCount),
                    },
                };
                double speed = speedModel.ComputeHeroSpeed(hero);
                if (troops < totalHorseCount)
                {
                    /*
                    this is in case there is enough mount for everyone soldier to be mounted.
                    The soldier will choose by default the fastest mounts they can find
                    In this case the speed of the army is the speed of the slowest mount among the used one
                    (which is worst of the top tier mounts) .
                    In this case the speed may not increase but should not decrease
                    */
                    Assert.LessOrEqual(speed, previousSpeed);
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
                Hero hero = new()
                {
                    Troops = 1000,
                    Items = new List<HeroItem>
                    {
                        HeroItemMount(450, 6 * mountCountFactor),
                        HeroItemMount(350, 2 * mountCountFactor),
                        HeroItemMount(250, 2 * mountCountFactor),
                    },
                };
                double speed = speedModel.ComputeHeroSpeed(hero);
                Assert.Greater(speed, previousSpeed);
                previousSpeed = speed;
            }
        }

        private HeroItem HeroItemMount(int hitPoints, int count)
        {
            return new()
            {
                Item = new Item { Mount = new ItemMountComponent { HitPoints = hitPoints } },
                Count = count,
            };
        }
    }
}
