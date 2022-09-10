﻿using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;

namespace Crpg.Domain.Entities.Battles;

/// <summary>
/// Fighter that joined their army to a <see cref="Battle"/> during the <see cref="BattlePhase.Preparation"/>
/// phase. Fighter can be either a <see cref="Party"/> or a <see cref="Settlement"/>. Not to be confused with
/// <see cref="BattleMercenary"/>.
/// </summary>
public class BattleFighter
{
    public int Id { get; set; }
    public int BattleId { get; set; }

    /// <summary>
    /// The id of the party that joined the <see cref="Battle"/>. Null if <see cref="BattleFighter"/>
    /// represents a <see cref="Settlement"/>.
    /// </summary>
    public int? PartyId { get; set; }

    /// <summary>
    /// The id of the settlement that being siege in the <see cref="Battle"/>. Null if <see cref="BattleFighter"/>
    /// represents a <see cref="Party"/>.
    /// </summary>
    public int? SettlementId { get; set; }

    public BattleSide Side { get; set; }

    /// <summary>
    /// Is the <see cref="BattleFighter"/> the fighter that initiated the <see cref="Battle"/>. There is one commander
    /// by <see cref="BattleSide"/>. If <see cref="SettlementId"/> is not null then it is guarantee that
    /// <see cref="Commander"/> is true.
    /// </summary>
    public bool Commander { get; set; }

    /// <summary>
    /// Maximum number of <see cref="BattleMercenary"/> for the <see cref="BattleFighter"/>. Depends on
    /// <see cref="Party"/> troops. This number doesn't include the <see cref="BattleFighter"/> itself.
    /// </summary>
    public int MercenarySlots { get; set; }

    public Party? Party { get; set; }
    public Settlement? Settlement { get; set; }
    public Battle? Battle { get; set; }
}
