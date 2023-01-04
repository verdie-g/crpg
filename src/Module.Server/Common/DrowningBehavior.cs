using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/// <summary>Inflict damages to agents under water level.</summary>
internal class DrowningBehavior : MissionLogic
{
    private const int TicksPerSecond = 2;
    private const int DamagePerSecond = 10;
    private static readonly MissionTime DelayBeforeDamage = MissionTime.Seconds(5f);

    private readonly Dictionary<int, MissionTime> _underwaterAgents;
    private MissionTimer? _tickTimer;
    private float _waterLevel;

    public DrowningBehavior()
    {
        _underwaterAgents = new Dictionary<int, MissionTime>();
        _waterLevel = float.MinValue;
    }

    public override void OnBehaviorInitialize()
    {
        _waterLevel = Mission.Scene.GetWaterLevel();
    }

    public override void OnMissionTick(float dt)
    {
        _tickTimer ??= new MissionTimer(1f / TicksPerSecond);
        if (!_tickTimer.Check(reset: true))
        {
            return;
        }

        // Killing an agent removes it from the Mission.Agents list which breaks its enumerator. So a temporary buffer
        // need to be used.
        List<Agent>? drowningAgents = null;
        foreach (var agent in Mission.Agents)
        {
            if (!agent.IsActive()
                || agent.GetChestGlobalPosition().Z > _waterLevel)
            {
                _underwaterAgents.Remove(agent.Index);
                continue;
            }

            if (!_underwaterAgents.TryGetValue(agent.Index, out var underwaterSince))
            {
                underwaterSince = MissionTime.Now;
                _underwaterAgents[agent.Index] = underwaterSince;
            }

            if (MissionTime.Now < underwaterSince + DelayBeforeDamage)
            {
                continue;
            }

            drowningAgents ??= new List<Agent>();
            drowningAgents.Add(agent);
        }

        if (drowningAgents == null)
        {
            return;
        }

        int damage = DamagePerSecond / TicksPerSecond;
        foreach (var agent in drowningAgents)
        {
            DamageHelper.DamageAgent(agent, damage);
        }
    }
}
