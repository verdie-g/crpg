using System.Collections.Generic;
using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Strategus
{
    public class StrategusHero : AuditableEntity
    {
        /// <summary>
        /// Same as <see cref="Users.User.Id"/> (one-to-one mapping).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The <see cref="Region"/> in which the user is playing.
        /// </summary>
        public Region Region { get; set; }

        /// <summary>
        /// Money of the user on Strategus. Different from <see cref="Users.User.Gold"/>.
        /// </summary>
        public int Gold { get; set; }

        /// <summary>
        /// Number of troops the user recruited.
        /// </summary>
        /// <remarks>Type is a float to be able to add a fraction of troop for each strategus tick.</remarks>
        public float Troops { get; set; }

        /// <summary>
        /// User position on the strategus map.
        /// </summary>
        public Point Position { get; set; } = default!;

        /// <summary>
        /// Status of the user. This property is used to interpret <see cref="Waypoints"/>, <see cref="TargetedHero"/> and
        /// <see cref="TargetedSettlement"/>.
        /// </summary>
        public StrategusHeroStatus Status { get; set; }

        /// <summary>
        /// Sequence of points the user is moving to if <see cref="Status"/> == <see cref="StrategusHeroStatus.MovingToPoint"/>.
        /// </summary>
        public MultiPoint Waypoints { get; set; } = MultiPoint.Empty;

        /// <summary>
        /// The id of the hero to follow if <see cref="Status"/> == <see cref="StrategusHeroStatus.FollowingHero"/>.
        /// The id of the hero to attack if <see cref="Status"/> == <see cref="StrategusHeroStatus.MovingToAttackHero"/>.
        /// </summary>
        public int? TargetedHeroId { get; set; }

        /// <summary>
        /// The id of the settlement the hero is staying in if <see cref="Status"/> == <see cref="StrategusHeroStatus.IdleInSettlement"/>.
        /// The id of the settlement the hero is moving to if <see cref="Status"/> == <see cref="StrategusHeroStatus.MovingToSettlement"/>.
        /// The id of the settlement to attack if <see cref="Status"/> == <see cref="StrategusHeroStatus.MovingToAttackSettlement"/>.
        /// </summary>
        public int? TargetedSettlementId { get; set; }

        /// <summary>See <see cref="TargetedHeroId"/>.</summary>
        public StrategusHero? TargetedHero { get; set; }

        /// <summary>See <see cref="TargetedSettlementId"/>.</summary>
        public StrategusSettlement? TargetedSettlement { get; set; }

        public User? User { get; set; }
        public List<StrategusOwnedItem>? OwnedItems { get; set; }
        public List<StrategusSettlement>? OwnedSettlements { get; set; }
    }
}
