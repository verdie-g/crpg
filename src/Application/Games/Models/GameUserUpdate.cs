using System;
using System.Collections.Generic;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Games.Models
{
    public class GameUserUpdate
    {
        public int CharacterId { get; set; }
        public GameUserReward Reward { get; set; } = new GameUserReward();
        public IList<GameUserBrokenItem> BrokenItems { get; set; } = Array.Empty<GameUserBrokenItem>();
    }
}
