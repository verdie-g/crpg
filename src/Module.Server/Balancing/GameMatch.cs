namespace Crpg.Module.Balancing;

internal class GameMatch
{
    public int TotalCount => TeamA.Count + TeamB.Count + Waiting.Count;
    internal List<WeightedCrpgUser> TeamA { get; set; } = new();
    internal List<WeightedCrpgUser> TeamB { get; set; } = new();
    internal List<WeightedCrpgUser> Waiting { get; set; } = new();
}
internal class ClanGroupsGameMatch
{
    internal List<ClanGroup> TeamA { get; set; } = new();
    internal List<ClanGroup> TeamB { get; set; } = new();
    internal List<ClanGroup> Waiting { get; set; } = new();
}
