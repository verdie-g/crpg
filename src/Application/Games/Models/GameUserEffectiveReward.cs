﻿namespace Crpg.Application.Games.Models;

/// <summary>
/// Because the user can have an experience multiplier of have skipped the fun the effective reward can be
/// different from what is received from the game server.
/// </summary>
public record GameUserEffectiveReward : GameUserReward
{
    public bool LevelUp { get; init; }
}
