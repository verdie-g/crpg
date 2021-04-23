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
            for (int i = 1; i <= 100; i++)
            {
                var hero1 = new StrategusHero
                {
                    Troops = 10 * i,
                    OwnedItems = new List<StrategusOwnedItem>
                    {
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 450 } }, Count = 150
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 350 } }, Count = 100
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 250 } }, Count = 50
                         }
                    }
                };
                var hero2 = new StrategusHero
                {
                    Troops = 10 * (i - 1),
                    OwnedItems = new List<StrategusOwnedItem>
                    {
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 450 } }, Count = 150
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 350 } }, Count = 100
                         },
                         new StrategusOwnedItem()
                         {
                             Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 150 } }, Count = 50
                         }
                    }
                };
                var speedModel = new StrategusSpeedModel();
                if (i < 21)
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