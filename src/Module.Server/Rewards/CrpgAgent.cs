namespace Crpg.Module.Rewards;
internal class CrpgAgent
{
    public int CharacterId { get; set; }
    public int CurrentHealth { get; set; }
    public int BaseHealthLimit { get; set; }
    public Dictionary<int, Hitter> Hitters { get; set; }
    public CrpgAgent(int characterId, int baseHealthLimit)
    {
        CharacterId = characterId;
        CurrentHealth = baseHealthLimit;
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
