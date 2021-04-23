﻿using System.Collections.Generic;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Strategus;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services
{
    public class StrategusSpeedModelTest // useless comment
    {
        private StrategusOwnedItem StrategusOwnedItemMount(int hitPoints, int count)
        {
            return
            new StrategusOwnedItem()
            {
                Item = new Item() { Mount = new ItemMountComponent() { HitPoints = hitPoints } },
                Count = count
            };
        }

        [Test]
        public void TroopsShouldUseTheBestMountTheyHave()
        {
            var hero1 = new StrategusHero
            {
                Troops = 10,
                OwnedItems = new List<StrategusOwnedItem>
                    {
                         StrategusOwnedItemMount(450, 5), StrategusOwnedItemMount(350, 5), StrategusOwnedItemMount(250, 5)
                    }
            };
            var hero2 = new StrategusHero
            {
                Troops = 10,
                OwnedItems = new List<StrategusOwnedItem>
                    {
                        StrategusOwnedItemMount(450, 5), StrategusOwnedItemMount(350, 10), StrategusOwnedItemMount(250, 10)
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
            double previousspeed = double.MaxValue;
            for (int troups = 10; troups <= 1000; troups += 10)
            {
                var hero1 = new StrategusHero
                {
                    Troops = troups,
                    OwnedItems = new List<StrategusOwnedItem>
                    {
                        StrategusOwnedItemMount(450, fastHorseCount), StrategusOwnedItemMount(350, mediumSpeedHorseCount), StrategusOwnedItemMount(250, slowHorseCount)
                    }
                };
                var speedModel = new StrategusSpeedModel();
                double speed = speedModel.ComputeHeroSpeed(hero1);
                if (troups < totalHorseCount)
                {
                    Assert.LessOrEqual(speed, previousspeed);
                }
                else
                {
                    Assert.Less(speed, previousspeed);
                }

                previousspeed = speed;
            }
        }

        [Test]
        public void BuyingMountsShouldIncreaseSpeed()
        {
            double previousspeed = 0;
            for (int i = 0; i < 100; i++)
            {
                var hero1 = new StrategusHero
                {
                    Troops = 1000,
                    OwnedItems = new List<StrategusOwnedItem>
                    {
                        StrategusOwnedItemMount(450, 7 * i), StrategusOwnedItemMount(350, 3 * i), StrategusOwnedItemMount(250, i)
                    }
                };
                var speedModel = new StrategusSpeedModel();
                var speed = speedModel.ComputeHeroSpeed(hero1);
                Assert.Greater(speed, previousspeed);
                previousspeed = speed;
            }
        }
    }
}