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
        public void TroopsShouldUseTheBestMountTheyhave()
        {
            var hero1 = new StrategusHero
            {
                Troops = 10,
                OwnedItems = new List<StrategusOwnedItem>
                    {
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 450 } }, Count = 5
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 350 } }, Count = 5
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 150 } }, Count = 5
                         }
                    }
            };
            var hero2 = new StrategusHero
            {
                Troops = 10,
                OwnedItems = new List<StrategusOwnedItem>
                    {
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 450 } }, Count = 5
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 350 } }, Count = 10
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 150 } }, Count = 10
                         }
                    }
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
            for (int troups = 10; troups <= 1000; troups += 10)
            {
                var hero1 = new StrategusHero
                {
                    Troops = troups,
                    OwnedItems = new List<StrategusOwnedItem>
                    {
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 450 } }, Count = fastHorseCount
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 350 } }, Count = mediumSpeedHorseCount
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 250 } }, Count = slowHorseCount
                         }
                    }
                };
                var hero2 = new StrategusHero
                {
                    Troops = troups - 10,
                    OwnedItems = new List<StrategusOwnedItem>
                    {
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 450 } }, Count = fastHorseCount
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 350 } }, Count = mediumSpeedHorseCount
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 250 } }, Count = slowHorseCount
                         }
                    }
                };
                var speedModel = new StrategusSpeedModel();
                if (troups < totalHorseCount)
                {
                    Assert.LessOrEqual(speedModel.ComputeHeroSpeed(hero1), speedModel.ComputeHeroSpeed(hero2));
                }
                else
                {
                    Assert.Less(speedModel.ComputeHeroSpeed(hero1), speedModel.ComputeHeroSpeed(hero2));
                }
            }
        }

        [Test]
        public void BuyingMountsShouldIncreaseSpeed()
        {
            for (int i = 0; i < 100; i++)
            {
                var hero1 = new StrategusHero
                {
                    Troops = 1000,
                    OwnedItems = new List<StrategusOwnedItem>
                    {
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 450 } }, Count = 7 * i
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 350 } }, Count = 3 * i
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 250 } }, Count = 1 * i
                         }
                    }
                };
                var hero2 = new StrategusHero
                {
                    Troops = 1000,
                    OwnedItems = new List<StrategusOwnedItem>
                    {
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 450 } }, Count = 7 * (i - 1)
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 350 } }, Count = 3 * (i - 1)
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 150 } }, Count = 1 * (i - 1)
                         }
                    }
                };
                var speedModel = new StrategusSpeedModel();
                Assert.Greater(speedModel.ComputeHeroSpeed(hero1), speedModel.ComputeHeroSpeed(hero2));
            }
        }
    }
}