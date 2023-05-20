namespace Crpg.Module.Rewards;
internal class AgentHitRegistry
{
    public int UserId { get; set; }
    public int BaseHealthLimit { get; set; }
    public Dictionary<int, int> DamageByUserId { get; set; }
    public AgentHitRegistry(int userId, int baseHealthLimit)
    {
        UserId = userId;
        BaseHealthLimit = baseHealthLimit;
        DamageByUserId = new();
    }
}
