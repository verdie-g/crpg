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
        _tickTimer ??= new MissionTimer(1f / TicksPerSecond);
        if (!_tickTimer.Check(reset: true))
        {
            return;
        }

        MatrixFrame entityFrame = GameEntity.GetGlobalFrame();
        Vec3 entityOrigin = entityFrame.origin;
        Vec3 entityScale = entityFrame.GetScale();
        entityFrame.rotation.ApplyScaleLocal(new Vec3(1f / entityScale.x, 1f / entityScale.y, 1f / entityScale.z));

        // Killing an agent removes it from the Mission.Agents list which breaks its enumerator. So a temporary buffer
        // need to be used.
        List<Agent>? agentsToDamage = null;
        foreach (var agent in Mission.Current.GetAgentsInRange(entityOrigin.AsVec2, entityScale.AsVec2.Length / 2))
        {
            if (!agent.IsActive())
            {
                _agentsInZone.Remove(agent.Index); // TODO: check go in, out, die, come back with same index.
                continue;
            }

            // GetAgentsInRange return agents in a circle but the damage zone is expected to be a cuboid, so double
            // check it is effectively inside it.
            Vec3 agentPosition = agent.GetChestGlobalPosition();
            Vec3 localAgentPosition = entityFrame.TransformToLocal(agentPosition);
            if (MathF.Abs(localAgentPosition.x) > entityScale.x / 2f
                || MathF.Abs(localAgentPosition.y) > entityScale.y / 2f
                || MathF.Abs(localAgentPosition.z) > entityScale.z)
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
