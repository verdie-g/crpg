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
        public void BuyingAMountShouldNotDecreaseSpeed()
        {
            var hero1 = new StrategusHero() { Troops = 3 };
            var hero2 = new StrategusHero() { Troops = 3 };
            var hero3 = new StrategusHero() { Troops = 3 };
            var mount1 = new StrategusOwnedItem() { Item = new Item() { Mount = new ItemMountComponent() { HitPoints = 250 } } };
            hero1.OwnedItems = new List<StrategusOwnedItem>();
            hero1.OwnedItems.Add(mount1);
            hero2.OwnedItems = new List<StrategusOwnedItem>();
            hero2.OwnedItems.Add(mount1);
            hero2.OwnedItems.Add(mount1);
            hero3.OwnedItems = new List<StrategusOwnedItem>();
            hero3.OwnedItems.Add(mount1);
            hero3.OwnedItems.Add(mount1);
            hero3.OwnedItems.Add(mount1);
            var speedModel = new StrategusSpeedModel();
            Assert.Greater(speedModel.ComputeHeroSpeed(hero2), speedModel.ComputeHeroSpeed(hero1));
            Assert.Greater(speedModel.ComputeHeroSpeed(hero3), speedModel.ComputeHeroSpeed(hero2));
        }
    }
}
