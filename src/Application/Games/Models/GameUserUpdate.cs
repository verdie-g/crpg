using System;
using System.Collections.Generic;

namespace Crpg.Application.Games.Models
{
    public record GameUserUpdate
    {
        public int CharacterId { get; init; }
        public GameUserReward Reward { get; init; } = new();
        public IList<GameUserBrokenItem> BrokenItems { get; init; } = Array.Empty<GameUserBrokenItem>();
    }
}
