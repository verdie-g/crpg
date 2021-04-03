using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crpg.Domain.Entities.Strategus;

namespace Crpg.Application.Common.Services
{
    /// <summary>
    /// Service to compute the speed of a <see cref="StrategusHero"/>.
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
            double terrainSpeedFactor = 1;
            double weightFactor = 1;
            /*
                      Troops        | troopInfluence |
            ------------------------+----------------+
             1                      |        2=2/1   |
             100                    |        1=2/2   |
             1000                   |          2/3   |
             10000                  |          2/4   |
            this divide the speed of the army by the order of magnitude of its size.
            10000 is four zeros so the denominator is 4 */
            double troopInfluence = 2 / (1 + Math.Log10(1 + hero.Troops / 10));
            return terrainSpeedFactor * weightFactor * SlowestMountSpeed(hero.Troops, hero.OwnedItems!) * troopInfluence;
        }

        private double SlowestMountSpeed(float troops, List<StrategusOwnedItem> ownedItems)
        {
            int mounts = 0;
            double forcedMarchSpeed = 2;
            foreach (StrategusOwnedItem ownedItem in ownedItems.OrderByDescending(i => i.Item!.Mount!.HitPoints))
            {
                mounts += ownedItem.Count;
                if (mounts >= troops)
                {
                    /*
                    this is in case there is enough mount for everyone soldier to be mounted.
                    The soldier will choose by default the fastest mounts they can find
                    In this case the speed of the army is the speed of the slowest mount among the used one
                    (which is worst of the top tier mounts) .Currently we're using the hitpoints
                    to calculate the speed, because strategus is about sustained speed.
                    Marathon runner are more suited for long distance than sprint runners
                    Manually designed speed for mounts should be added later for more fine tuning
                    */
                    return ownedItem.Item!.Mount!.HitPoints / 100;
                }
            }

            /*
            this is in case there is not enough mounts for every soldier to be mounted
            the model for this is assuming some of the soldiers have to walk.
            Since they can change places with someone that is already on a mount, they can afford to walk faster
            the more the ratio troops / mounts is close to 1 , the more they can afford.
            */
            return forcedMarchSpeed * troops / mounts + (1 - troops / mounts);
        }
    }
}
