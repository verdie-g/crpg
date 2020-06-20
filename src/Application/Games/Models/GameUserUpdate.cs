﻿using System;
using System.Collections.Generic;

namespace Crpg.Application.Games.Models
{
    public class GameUserUpdate
    {
        public long SteamId { get; set; }
        public string CharacterName { get; set; } = default!;
        public GameUserReward? Reward { get; set; }
        public IList<GameUserBrokenItem> BrokenItems { get; set; } = Array.Empty<GameUserBrokenItem>();
    }
}