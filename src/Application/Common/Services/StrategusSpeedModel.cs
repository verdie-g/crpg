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
        double ComputeHeroSpeed(StrategusHero hero, double terrainSpeedFactor, double weightFactor);
    }

    internal class StrategusSpeedModel : IStrategusSpeedModel
    {
        /// <inheritdoc />
        public double ComputeHeroSpeed(StrategusHero hero, double terrainSpeedFactor = 1.0, double weightFactor = 1.0)
        {
            // const double terrainSpeedFactor = 1.0; // TODO: terrain penalty on speed (e.g. lower speed in forest).
            // const double weightFactor = 1.0; // TODO: goods should slow down hero
            double troopInfluence = 2 / (1 + Math.Log10(1 + hero.Troops / 10));
            return terrainSpeedFactor * weightFactor * SlowestHorseSpeed(hero.Troops, hero.OwnedItems!) * troopInfluence;
        }


        private double SlowestHorseSpeed(float numberOfTroops, List<StrategusOwnedItem> owneditems ,double forceMarchSpeed = 2)
             {
            // The table should have in its first column the type of horse , and in its second column how many there is
            // Courser | 12
            // palfrey | 5
            int horses = 0;
            if (owneditems != null)
            {
                 foreach (StrategusOwnedItem ownedItem in owneditems.OrderBy(i => i.Item!.Mount!.HitPoints))
                     {
                if (horses < numberOfTroops)
                    {
                        horses += ownedItem.Count;
                    }
                    else
                    {
                        return ownedItem.Item!.Mount!.HitPoints / 100;
                    }
                }
            }

            return forceMarchSpeed * horses / numberOfTroops + (1 - horses / numberOfTroops);
             }
    }
}
