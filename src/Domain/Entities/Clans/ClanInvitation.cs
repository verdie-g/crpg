using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Clans
{
    /// <summary>
    /// An invitation or a request to join a clan.
    /// </summary>
    public class ClanInvitation : AuditableEntity
    {
        public int Id { get; set; }
        public int ClanId { get; set; }

        /// <summary>
        /// Id of the <see cref="User"/> that was invited or that requested to join a <see cref="Clan"/>.
        /// </summary>
        public int InviteeUserId { get; set; }

        /// <summary>
        /// Id of the <see cref="User"/> that created/accepted/declined the invitation, depending on <see cref="Type"/>
        /// and <see cref="Status"/>.
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       If <see cref="Type"/> == <see cref="ClanInvitationType.Offer"/> then it's the id of the user that offered
        ///       the invitation.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       If <see cref="Type"/> == <see cref="ClanInvitationType.Request"/> &amp;&amp; <see cref="Status"/> ==
        ///       <see cref="ClanInvitationStatus.Pending"/> then it is equal to <see cref="InviteeUserId"/>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       If <see cref="Type"/> == <see cref="ClanInvitationType.Request"/> &amp;&amp; (<see cref="Status"/> ==
        ///       <see cref="ClanInvitationStatus.Accepted"/> || <see cref="Status"/> == <see cref="ClanInvitationStatus.Declined"/>)
        ///       then it's the id of the user that accepted/declined the invitation.
        ///     </description>
        ///   </item>
        /// </list>
        /// </summary>
        public int InviterUserId { get; set; }

        /// <summary>
        /// Type of the <see cref="ClanInvitation"/> (request or offer).
        /// </summary>
        public ClanInvitationType Type { get; set; }

        /// <summary>
        /// Status of the <see cref="ClanInvitation"/>.
        /// </summary>
        public ClanInvitationStatus Status { get; set; }

        public Clan? Clan { get; set; }
        public User? InviteeUser { get; set; }

        /// <summary>See <see cref="InviterUserId"/>.</summary>
        public User? InviterUser { get; set; }
    }
}
