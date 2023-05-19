namespace Crpg.Module.Rewards;
internal class AgentHitRegistry
{
    public int UserId { get; set; }
    public int BaseHealthLimit { get; set; }
    public bool IsTeamHit { get; set; }
    public Dictionary<int, int> DamageByUserId { get; set; }
    public AgentHitRegistry(int userId, int baseHealthLimit, bool isTeamHit)
    {
        UserId = userId;
        BaseHealthLimit = baseHealthLimit;
        IsTeamHit = isTeamHit;
        DamageByUserId = new();
    }
}
