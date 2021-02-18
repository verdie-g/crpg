using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Strategus
{
    public class StrategusUser : AuditableEntity
    {
        public int UserId { get; set; }

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
        /// Sequence of points the user is moving to.
        /// </summary>
        public MultiPoint Moves { get; set; } = default!; // TODO: Follow/Attack user?

        public User? User { get; set; }
        public StrategusSettlement? OwnedSettlements { get; set; }
    }
}
