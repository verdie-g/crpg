using System;
using System.Collections.Generic;

namespace Crpg.Application.Games.Models
{
    public class RewardResponse
    {
        public IList<UserRewardResponse> Users { get; set; } = Array.Empty<UserRewardResponse>();
    }
}