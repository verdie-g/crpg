using System;
using System.Collections.Generic;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Games.Models
{
    public class GameUserUpdate
    {
        public Platform Platform { get; set; }
        public string PlatformUserId { get; set; } = default!;
        public string CharacterName { get; set; } = default!;
        public GameUserReward? Reward { get; set; }
        public IList<GameUserBrokenItem> BrokenItems { get; set; } = Array.Empty<GameUserBrokenItem>();
    }
}
