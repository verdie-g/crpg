using Crpg.Domain.Common;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Parties;

/// <summary>
/// A party is a user information on the strategus map.
/// </summary>
public class Party : AuditableEntity
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
    public Point Position { get; set; } = Point.Empty;

    /// <summary>
    /// Status of the user. This property is used to interpret <see cref="Waypoints"/>, <see cref="TargetedParty"/> and
    /// <see cref="TargetedSettlement"/>.
    /// </summary>
    public PartyStatus Status { get; set; }

    /// <summary>
    /// Sequence of points the user is moving to if <see cref="Status"/> == <see cref="PartyStatus.MovingToPoint"/>.
    /// </summary>
    public MultiPoint Waypoints { get; set; } = MultiPoint.Empty;

    /// <summary>
    /// The id of the party to follow if <see cref="Status"/> == <see cref="PartyStatus.FollowingParty"/>.
    /// The id of the party to attack if <see cref="Status"/> == <see cref="PartyStatus.MovingToAttackParty"/>.
    /// </summary>
    public int? TargetedPartyId { get; set; }

    /// <summary>
    /// The id of the settlement the party is staying in if <see cref="Status"/> == <see cref="PartyStatus.IdleInSettlement"/>.
    /// The id of the settlement the party is recruiting in if <see cref="Status"/> == <see cref="PartyStatus.RecruitingInSettlement"/>.
    /// The id of the settlement the party is moving to if <see cref="Status"/> == <see cref="PartyStatus.MovingToSettlement"/>.
    /// The id of the settlement to attack if <see cref="Status"/> == <see cref="PartyStatus.MovingToAttackSettlement"/>.
    /// </summary>
    public int? TargetedSettlementId { get; set; }

    /// <summary>See <see cref="TargetedPartyId"/>.</summary>
    public Party? TargetedParty { get; set; }

    /// <summary>See <see cref="TargetedSettlementId"/>.</summary>
    public Settlement? TargetedSettlement { get; set; }

    public User? User { get; set; }
    public List<PartyItem> Items { get; set; } = new();
    public List<Settlement> OwnedSettlements { get; set; } = new();
}
