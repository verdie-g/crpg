using Crpg.Domain.Entities.Strategus.Battles;

namespace Crpg.Domain.Entities.Strategus
{
    public enum StrategusHeroStatus
    {
        /// <summary>
        /// Inactive.
        /// </summary>
        Idle,

        /// <summary>
        /// Inactive in a settlement.
        /// </summary>
        IdleInSettlement,

        /// <summary>
        /// Recruiting troops in a <see cref="StrategusSettlement"/>.
        /// </summary>
        RecruitingInSettlement,

        /// <summary>
        /// Moving to an arbitrary location.
        /// </summary>
        MovingToPoint,

        /// <summary>
        /// Following a <see cref="StrategusHero"/>.
        /// </summary>
        FollowingHero,

        /// <summary>
        /// Following a <see cref="StrategusSettlement"/>.
        /// </summary>
        MovingToSettlement,

        /// <summary>
        /// Moving to attack a <see cref="StrategusHero"/>.
        /// </summary>
        MovingToAttackHero,

        /// <summary>
        /// Moving to attack a <see cref="StrategusSettlement"/>.
        /// </summary>
        MovingToAttackSettlement,

        /// <summary>
        /// Stationary because involved in a <see cref="StrategusBattle"/>.
        /// </summary>
        InBattle,
    }
}
