namespace Crpg.Module.Api.Models.Restrictions;

// Copy of Crpg.Application.Games.Models.Restrictions.Restriction.
internal class CrpgRestriction
{
    public int Id { get; set; }
    // public UserPublicViewModel? RestrictedUser { get; set; }
    public TimeSpan Duration { get; set; }
    public CrpgRestrictionType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    // public UserPublicViewModel? RestrictedByUser { get; set; }
    public DateTime CreatedAt { get; set; }
}
