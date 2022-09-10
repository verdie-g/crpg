﻿using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;

namespace Crpg.Domain.Entities.Battles;

/// <summary>
/// Phase of a <see cref="Battle"/>.
/// </summary>
public enum BattlePhase
{
    /// <summary>
    /// A <see cref="Party"/> attacked another one or a <see cref="Settlement"/>. Other <see cref="Party"/>es
    /// can join their army during this phase as <see cref="BattleFighter"/>.
    /// </summary>
    Preparation,

    /// <summary>
    /// <see cref="Party"/>es from anywhere on the map can join the <see cref="Battle"/>
    /// as <see cref="BattleMercenary"/>.
    /// </summary>
    Hiring,

    /// <summary>
    /// The <see cref="Battle"/> was scheduled to a certain date.
    /// </summary>
    Scheduled,

    /// <summary>
    /// The <see cref="Battle"/> is live on a server.
    /// </summary>
    Live,

    /// <summary>
    /// The <see cref="Battle"/> ended.
    /// </summary>
    End,
}
