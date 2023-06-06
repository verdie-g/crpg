using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Balancing;

internal class GameMatch
{
    internal List<WeightedCrpgUser> TeamA { get; set; } = new();
    internal List<WeightedCrpgUser> TeamB { get; set; } = new();
    internal List<WeightedCrpgUser> Waiting { get; set; } = new();
}

internal class DTVGameMatch
{
    internal List<CrpgUser> TeamA { get; set; } = new();
    internal List<CrpgUser> TeamB { get; set; } = new();
    internal List<CrpgUser> Waiting { get; set; } = new();
}

internal class ClanGroupsGameMatch
{
    internal List<ClanGroup> TeamA { get; set; } = new();
    internal List<ClanGroup> TeamB { get; set; } = new();
    internal List<ClanGroup> Waiting { get; set; } = new();
}
