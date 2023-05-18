namespace Crpg.Module.Rewards;
internal class AgentHitRegistry
{
    public int CharacterId { get; set; }
    public int BaseHealthLimit { get; set; }
    public Dictionary<int, Hitter> Hitters { get; set; }
    public AgentHitRegistry(int characterId, int baseHealthLimit)
    {
        CharacterId = characterId;
        BaseHealthLimit = baseHealthLimit;
        Hitters = new();
    }

    internal class Hitter
    {
        public int CharacterId { get; set; }
        public int TotalDamageDone { get; set; }
        public bool IsSameTeam { get; set; }
    }
}
