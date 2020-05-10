using System;
using System.Collections.Generic;

namespace Crpg.GameMod.Api.Responses
{
    /// <summary>
    /// Copy of Crpg.Application.Games.Models.RewardResponse.
    /// </summary>
    public class RewardResponse
    {
        public IList<UserRewardResponse> Users { get; set; } = Array.Empty<UserRewardResponse>();
    }

    /// <summary>
    /// Copy of Crpg.Application.Games.Models.UserRewardResponse.
    /// </summary>
    public class UserRewardResponse
    {
        public int UserId { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public int Gold { get; set; }
    }
}