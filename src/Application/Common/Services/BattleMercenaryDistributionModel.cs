using System;
using System.Collections.Generic;
using System.Linq;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Common.Services
{
    internal interface IBattleMercenaryDistributionModel
    {
        /// <summary>
        /// Sets <see cref="BattleFighter.MercenarySlots"/> for each <see cref="BattleFighter"/> of a <see cref="Battle"/>.
        /// </summary>
        /// <param name="fighters">All fighters of the battle.</param>
        /// <param name="battleSlots">Max number of players in the battle for both teams.</param>
        void DistributeMercenaries(IEnumerable<BattleFighter> fighters, int battleSlots);
    }

    internal class BattleMercenaryUniformDistributionModel : IBattleMercenaryDistributionModel
    {
        public void DistributeMercenaries(IEnumerable<BattleFighter> fighters, int battleSlots)
        {
            foreach (IGrouping<BattleSide, BattleFighter> teamFighters in fighters.GroupBy(f => f.Side))
            {
                int teamTotalTroops = teamFighters.Sum(fighter => (int)fighter.Hero!.Troops);
                int remainingSlots = battleSlots;
                foreach (var fighter in teamFighters)
                {
                    // Compute the share the fighter troops represents in the entire army and give the fighter
                    // the same share of mercenary slots.
                    double fighterFactor = Math.Floor(fighter.Hero!.Troops) / teamTotalTroops;
                    int slotsForFighter = (int)(fighterFactor * battleSlots);
                    fighter.MercenarySlots = slotsForFighter - 1; // Remove one for the slot of the fighter itself.
                    remainingSlots -= slotsForFighter;
                }

                foreach (var fighter in teamFighters)
                {
                    if (remainingSlots == 0)
                    {
                        break;
                    }

                    fighter.MercenarySlots += 1;
                    remainingSlots -= 1;
                }
            }
        }
    }
}
