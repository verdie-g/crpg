namespace Crpg.Domain.Entities.Strategus
{
    public enum StrategusUserStatus
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
        /// Moving to an arbitrary location.
        /// </summary>
        MovingToPoint,

        /// <summary>
        /// Following a <see cref="StrategusUser"/>.
        /// </summary>
        FollowingUser,

        /// <summary>
        /// Following a <see cref="StrategusSettlement"/>.
        /// </summary>
        MovingToSettlement,

        /// <summary>
        /// Moving to attack a <see cref="StrategusUser"/>.
        /// </summary>
        AttackingUser,

        /// <summary>
        /// Moving to attack a <see cref="StrategusSettlement"/>.
        /// </summary>
        AttackingSettlement,

        /// <summary>
        /// Stationary because involved in a <see cref="StrategusBattle"/>.
        /// </summary>
        InBattle,
    }
}
