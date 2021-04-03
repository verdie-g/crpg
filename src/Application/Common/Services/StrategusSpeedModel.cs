using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crpg.Domain.Entities.Strategus;

namespace Crpg.Application.Common.Services
{
    /// <summary>
    /// Service to compute the speed of an <see cref="StrategusHero"/>.
    /// </summary>
    internal interface IStrategusSpeedModel
    {
        /// <summary>Compute the Hero Speed.</summary>
        double ComputeHeroSpeed(StrategusHero hero);
    }

    internal class StrategusSpeedModel : IStrategusSpeedModel
    {
        /// <inheritdoc />
        public double ComputeHeroSpeed(StrategusHero hero)
        {
            // const double terrainSpeedFactor = 1.0; // TODO: terrain penalty on speed (e.g. lower speed in forest).
            // const double weightFactor = 1.0; // TODO: goods should slow down hero
            double terrainSpeedFactor = 1;
            double weightFactor = 1;
            double troopInfluence = 2 / (1 + Math.Log10(1 + hero.Troops / 10));
            /*
                      Troops        | troopInfluence |
            ------------------------+----------------+
             1                      |        2=2/1   |
             100                    |        1=2/2   |
             1000                   |          2/3   |
             10000                  |          2/4   |
            */
            return terrainSpeedFactor * weightFactor * SlowestMountSpeed(hero.Troops, hero.OwnedItems!) * troopInfluence;
        }

        private double SlowestMountSpeed(float numberOfTroops, List<StrategusOwnedItem> owneditems)
        {
            int mounts = 0;
            double forceMarchSpeed = 2;
            if (owneditems.Any())
            {
                foreach (StrategusOwnedItem ownedItem in owneditems.OrderBy(i => i.Item!.Mount!.HitPoints))
                {
                    mounts += ownedItem.Count;
                    if (mounts >= numberOfTroops)
                    {
                        return ownedItem.Item!.Mount!.HitPoints / 100;
                        /*
                        this is in case there is enough mount for everyone soldier to be mounted. In this case the speed of the army is the speed of the slowest mount.
                        Currently we're using the hitpoints to calculate the speed , but manually designed speed for mounts should be added later
                        */
                    }
                }
            }

            /*
            this is in case there is not enough mounts for every soldier to be mounted
            the model for this is assuming some of the soldiers have to walk.
            Since they can change places with someone that is already on a mount, they can afford to walk faster
            the more the ratio numberOfTroops / mounts is close to 1 , the more they can afford.
            */
            return forceMarchSpeed * numberOfTroops / mounts + (1 - numberOfTroops / mounts);
        }
    }
}
