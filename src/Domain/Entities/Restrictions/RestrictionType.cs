namespace Crpg.Domain.Entities.Restrictions;

public enum RestrictionType
{
    /// <summary>Can't access cRPG.</summary>
    All,

    /// <summary>Can't join a server.</summary>
    Join,

    /// <summary>Can't write in chat.</summary>
    Chat,

    /// <summary>Can't write in chat.</summary>
    RatingReset,
}
