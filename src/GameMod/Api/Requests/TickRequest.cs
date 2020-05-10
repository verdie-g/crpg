using System;
using System.Collections.Generic;

namespace Crpg.GameMod.Api.Requests
{
    /// <summary>
    /// Copy of Crpg.Application.Games.Commands.RewardCommand.
    /// </summary>
    public class RewardRequest
    {
        public IList<UserReward> Users { get; set; } = Array.Empty<UserReward>();
    }

    /// <summary>
    /// Copy of Crpg.Application.Games.Models.UserReward.
    /// </summary>
    public class UserReward
    {
        public int CharacterId { get; set; }
        public int ExperienceGain { get; set; }
        public int GoldGain { get; set; }
    }
}