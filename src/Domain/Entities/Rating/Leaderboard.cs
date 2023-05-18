using System;
using System.Collections.Generic;
using System.Text;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Domain.Entities.Rating;
public class Leaderboard
{
    public DateTime? LeaderboardLastUpdatedDate { get; set; }
    public List<Character> LeaderboardList { get; set; } = new();
}
