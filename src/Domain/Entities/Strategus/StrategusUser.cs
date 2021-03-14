using System.Collections.Generic;
using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Strategus
{
    public class StrategusUser : AuditableEntity
    {
        public int UserId { get; set; }

        /// <summary>
        /// The <see cref="Region"/> in which the user is playing.
        /// </summary>
        public Region Region { get; set; }

        /// <summary>
        /// Money of the user on Strategus. Different from <see cref="Users.User.Gold"/>.
        /// </summary>
        public int Silver { get; set; }

        /// <summary>
        /// Number of troops the user recruited.
        /// </summary>
        public int Troops { get; set; }

        /// <summary>
        /// User position on the strategus map.
        /// </summary>
        public Point Position { get; set; } = default!;

        /// <summary>
        /// Status of the user. This property is used to interpret <see cref="Waypoints"/>, <see cref="TargetedUser"/> and
        /// <see cref="TargetedSettlement"/>.
        /// </summary>
        public StrategusUserStatus Status { get; set; }

        /// <summary>
        /// Sequence of points the user is moving to if <see cref="Status"/> == <see cref="StrategusUserStatus.MovingToPoint"/>.
        /// </summary>
        public MultiPoint Waypoints { get; set; } = MultiPoint.Empty;

        /// <summary>
        /// The id of the user to follow if <see cref="Status"/> == <see cref="StrategusUserStatus.FollowingUser"/>.
        /// The id of the user to attack if <see cref="Status"/> == <see cref="StrategusUserStatus.MovingToAttackUser"/>.
        /// </summary>
        public int? TargetedUserId { get; set; }

        /// <summary>
        /// The id of the settlement the user is staying in if <see cref="Status"/> == <see cref="StrategusUserStatus.IdleInSettlement"/>.
        /// The id of the settlement the user is moving to if <see cref="Status"/> == <see cref="StrategusUserStatus.MovingToSettlement"/>.
        /// The id of the settlement to attack if <see cref="Status"/> == <see cref="StrategusUserStatus.MovingToAttackSettlement"/>.
        /// </summary>
        public int? TargetedSettlementId { get; set; }

        /// <summary>See <see cref="TargetedUserId"/>.</summary>
        public StrategusUser? TargetedUser { get; set; }

        /// <summary>See <see cref="TargetedSettlementId"/>.</summary>
        public StrategusSettlement? TargetedSettlement { get; set; }

        public User? User { get; set; }
        public List<StrategusSettlement>? OwnedSettlements { get; set; }
    }
}
