using Crpg.Module.Common;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Scripts;

/// <summary>A script that damages agents inside the entity it is attached to.</summary>
internal class DamageZone : ScriptComponentBehavior
{
    private const int TicksPerSecond = 2;

#pragma warning disable SA1401 // Bannerlord editor expects fields
#pragma warning disable SA1202
    public int DamagePerSecond = 5;
    public float Delay = 2f;
#pragma warning restore SA1202
#pragma warning restore SA1401

#if CRPG_SERVER
    private readonly Dictionary<int, MissionTime> _agentsInZone = new();
    private MissionTimer? _tickTimer;

    public override TickRequirement GetTickRequirement()
    {
        return base.GetTickRequirement() | TickRequirement.Tick;
    }
#endif

    protected override void OnInit()
    {
        base.OnInit();
        Debug.Print($"Initialize {nameof(DamageZone)} script on entity {GameEntity.GetGuid()}");
        SetScriptComponentToTick(GetTickRequirement());
    }

#if CRPG_SERVER
    protected override void OnTick(float dt)
    {
        _tickTimer ??= new MissionTimer(Math.Max(1f / TicksPerSecond, 0.25f));
        if (!_tickTimer.Check(reset: true))
        {
            return;
        }

        var entityFrame = GameEntity.GetFrame();
        Vec3 entityOrigin = entityFrame.origin;
        Vec3 entityScale = entityFrame.GetScale();

        float entityXLo = entityOrigin.x - entityScale.x / 2f;
        float entityXHi = entityOrigin.x + entityScale.x / 2f;
        float entityYLo = entityOrigin.y - entityScale.y / 2f;
        float entityYHi = entityOrigin.y + entityScale.y / 2f;
        float entityZLo = entityOrigin.z;
        float entityZHi = entityOrigin.z + entityScale.z;

        // Killing an agent removes it from the Mission.Agents list which breaks its enumerator. So a temporary buffer
        // need to be used.
        List<Agent>? agentsToDamage = null;
        foreach (var agent in Mission.Current.Agents)
        {
            if (!agent.IsActive())
            {
                _agentsInZone.Remove(agent.Index);
                continue;
            }

            Vec3 agentPosition = agent.GetChestGlobalPosition();
            if (agentPosition.x < entityXLo
                || agentPosition.x > entityXHi
                || agentPosition.y < entityYLo
                || agentPosition.y > entityYHi
                || agentPosition.z < entityZLo
                || agentPosition.z > entityZHi)
            {
                _agentsInZone.Remove(agent.Index);
                continue;
            }

            if (!_agentsInZone.TryGetValue(agent.Index, out var inZoneSince))
            {
                inZoneSince = MissionTime.Now;
                _agentsInZone[agent.Index] = inZoneSince;
            }

            if (MissionTime.Now < inZoneSince + MissionTime.Seconds(Delay))
            {
                continue;
            }

            agentsToDamage ??= new List<Agent>();
            agentsToDamage.Add(agent);
        }

        if (agentsToDamage == null)
        {
            return;
        }

        int damage = DamagePerSecond / TicksPerSecond;
        foreach (var agent in agentsToDamage)
        {
            DamageHelper.DamageAgent(agent, damage);
        }
    }
#endif
}
