namespace Crpg.Module.Rewards;
internal class CrpgAgent
{
    public int CharacterId { get; set; }
    public int BaseHealthLimit { get; set; }
    public Dictionary<int, TeamHitter> TeamHitters { get; set; }
    public CrpgAgent(int characterId, int baseHealthLimit)
    {
        CharacterId = characterId;
        BaseHealthLimit = baseHealthLimit;
        TeamHitters = new();
    }

    public void SufferTeamHit(int affectorCharacterId, int inflictedDamage)
    {
        if (TeamHitters.TryGetValue(affectorCharacterId, out TeamHitter? affector) && affector != null)
        {
            affector.Damage = Math.Min(affector.Damage + inflictedDamage, BaseHealthLimit);
        }
        else
        {
            TeamHitters.Add(affectorCharacterId, new TeamHitter { Damage = inflictedDamage });
        }
    }

    internal class TeamHitter
    {
        public int Damage { get; set; }
    }
}
