using System.Collections.Generic;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Strategus;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services
{
    public class StrategusSpeedModelTest
    {
        [Test]
        public void TroopsShouldUseTheBestMountTheyHave()
        {
            var hero1 = new StrategusHero
            {
                Troops = 10,
                Items = new List<StrategusHeroItem>
                {
                    StrategusHeroItemMount(450, 5),
                    StrategusHeroItemMount(350, 5),
                    StrategusHeroItemMount(250, 5),
                },
            };
            var hero2 = new StrategusHero
            {
                Troops = 10,
                Items = new List<StrategusHeroItem>
                {
                    StrategusHeroItemMount(450, 5),
                    StrategusHeroItemMount(350, 10),
                    StrategusHeroItemMount(250, 10),
                },
            };
            var speedModel = new StrategusSpeedModel();
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
            var speedModel = new StrategusSpeedModel();
            for (int troops = 10; troops <= 1000; troops += 10)
            {
                var hero = new StrategusHero
                {
                    Troops = troops,
                    Items = new List<StrategusHeroItem>
                    {
                        StrategusHeroItemMount(450, fastHorseCount),
                        StrategusHeroItemMount(350, mediumSpeedHorseCount),
                        StrategusHeroItemMount(250, slowHorseCount),
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
            var speedModel = new StrategusSpeedModel();
            for (int mountCountFactor = 1; mountCountFactor <= 100; mountCountFactor++)
            {
                var hero = new StrategusHero
                {
                    Troops = 1000,
                    Items = new List<StrategusHeroItem>
                    {
                        StrategusHeroItemMount(450, 6 * mountCountFactor),
                        StrategusHeroItemMount(350, 2 * mountCountFactor),
                        StrategusHeroItemMount(250, 2 * mountCountFactor),
                    },
                };
                var speed = speedModel.ComputeHeroSpeed(hero);
                Assert.Greater(speed, previousSpeed);
                previousSpeed = speed;
            }
        }

        private StrategusHeroItem StrategusHeroItemMount(int hitPoints, int count)
        {
            return new()
            {
                Item = new Item { Mount = new ItemMountComponent { HitPoints = hitPoints } },
                Count = count,
            };
        }
    }
}
