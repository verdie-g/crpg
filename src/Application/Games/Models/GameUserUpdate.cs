using System;
using System.Collections.Generic;

namespace Crpg.Application.Games.Models
{
    public class GameUserUpdate
    {
        public string PlatformUserId { get; set; } = default!;
        public string CharacterName { get; set; } = default!;
        public GameUserReward? Reward { get; set; }
        public IList<GameUserBrokenItem> BrokenItems { get; set; } = Array.Empty<GameUserBrokenItem>();
    }
}