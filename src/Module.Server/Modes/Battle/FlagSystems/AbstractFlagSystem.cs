using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using MathF = TaleWorlds.Library.MathF;
using Timer = TaleWorlds.Core.Timer;

namespace Crpg.Module.Modes.Battle.FlagSystems;

internal abstract class AbstractFlagSystem
{
#pragma warning disable SA1401 // False negative.
    protected readonly Mission Mission;
#pragma warning restore SA1401

    private const float FlagCaptureRange = 6f;
    private const float FlagCaptureRangeSquared = FlagCaptureRange * FlagCaptureRange;

    /// <summary>Null on client-side.</summary>
    private readonly MultiplayerGameNotificationsComponent _notificationsComponent;
    private readonly CrpgBattleClient _battleClient;

    /// <summary>True if a random flag has been spawned.</summary>
    private bool _hasFlagCountChanged;
    private Timer? _checkFlagRemovalTimer;

    private FlagCapturePoint[] _flags = Array.Empty<FlagCapturePoint>();
    private Team?[] _flagOwners = Array.Empty<Team>();
    private int[,] _agentCountsAroundFlags = new int[0, 0];

    public AbstractFlagSystem(Mission mission, MultiplayerGameNotificationsComponent notificationsComponent, CrpgBattleClient battleClient)
    {
        Mission = mission;
        _battleClient = battleClient;
        _notificationsComponent = notificationsComponent;
    }

    public virtual void ResetFlags()
    {
        _flags = Mission.Current.MissionObjects.FindAllWithType<FlagCapturePoint>().ToArray();
        ThrowOnBadFlagIndexes(_flags);
        _flagOwners = new Team[_flags.Length];
        _agentCountsAroundFlags = new int[_flags.Length, (int)BattleSideEnum.NumSides];
        foreach (var flag in _flags)
        {
            ResetFlag(flag);
        }
    }

    public FlagCapturePoint[] GetAllFlags()
    {
        return _flags;
    }

    public FlagCapturePoint[] GetCapturedFlags()
    {
        return _flags
            .Where(flag => !flag.IsDeactivated && GetFlagOwner(flag) != null && flag.IsFullyRaised)
            .ToArray();
    }

    public FlagCapturePoint GetLastFlag()
    {
        return _flags.First(flag => !flag.IsDeactivated);
    }

    public bool HasNoFlags()
    {
        return _flags.Length == 0;
    }

    public Team? GetFlagOwner(FlagCapturePoint flag)
    {
        return _flagOwners[flag.FlagIndex];
    }

    public void TickFlags()
    {
        foreach (var flag in _flags)
        {
            if (flag.IsDeactivated)
            {
                continue;
            }

            int[] agentCountsAroundFlag = new int[(int)BattleSideEnum.NumSides];

            Agent? closestAgentToFlag = null;
            float closestAgentDistanceToFlagSquared = 16f; // Where does this number come from?
            var proximitySearch = AgentProximityMap.BeginSearch(Mission.Current, flag.Position.AsVec2, FlagCaptureRange);
            for (; proximitySearch.LastFoundAgent != null; AgentProximityMap.FindNext(Mission.Current, ref proximitySearch))
            {
                Agent agent = proximitySearch.LastFoundAgent;
                if (CanAgentCaptureFlag(agent))
                {
                    continue;
                }

                agentCountsAroundFlag[(int)agent.Team.Side] += 1;
                float distanceToFlagSquared = agent.Position.DistanceSquared(flag.Position);
                if (distanceToFlagSquared <= closestAgentDistanceToFlagSquared)
                {
                    closestAgentToFlag = agent;
                    closestAgentDistanceToFlagSquared = distanceToFlagSquared;
                }
            }

            bool agentCountsAroundFlagChanged =
                agentCountsAroundFlag[(int)BattleSideEnum.Defender] != _agentCountsAroundFlags[flag.FlagIndex, (int)BattleSideEnum.Defender]
                || agentCountsAroundFlag[(int)BattleSideEnum.Attacker] != _agentCountsAroundFlags[flag.FlagIndex, (int)BattleSideEnum.Attacker];
            _agentCountsAroundFlags[flag.FlagIndex, (int)BattleSideEnum.Defender] = agentCountsAroundFlag[(int)BattleSideEnum.Defender];
            _agentCountsAroundFlags[flag.FlagIndex, (int)BattleSideEnum.Attacker] = agentCountsAroundFlag[(int)BattleSideEnum.Attacker];
            float speedMultiplier = 1f;
            if (closestAgentToFlag != null)
            {
                BattleSideEnum side = closestAgentToFlag.Team.Side;
                BattleSideEnum oppositeSide = side.GetOppositeSide();
                if (agentCountsAroundFlag[(int)oppositeSide] != 0)
                {
                    int val1 = Math.Min(agentCountsAroundFlag[(int)side], 200);
                    int val2 = Math.Min(agentCountsAroundFlag[(int)oppositeSide], 200);
                    speedMultiplier = Math.Min(1f, (MathF.Log10(val1) + 1.0f) / (2.0f * (MathF.Log10(val2) + 1.0f)) - 0.09f);
                }
            }

            var flagOwner = GetFlagOwner(flag);
            if (flagOwner == null)
            {
                if (!flag.IsContested && closestAgentToFlag != null)
                {
                    flag.SetMoveFlag(CaptureTheFlagFlagDirection.Down, speedMultiplier);
                }
                else if (closestAgentToFlag == null & flag.IsContested)
                {
                    flag.SetMoveFlag(CaptureTheFlagFlagDirection.Up, speedMultiplier);
                }
                else if (agentCountsAroundFlagChanged)
                {
                    flag.ChangeMovementSpeed(speedMultiplier);
                }
            }
            else if (closestAgentToFlag != null)
            {
                if (closestAgentToFlag.Team != flagOwner && !flag.IsContested)
                {
                    flag.SetMoveFlag(CaptureTheFlagFlagDirection.Down, speedMultiplier);
                }
                else if (closestAgentToFlag.Team == flagOwner & flag.IsContested)
                {
                    flag.SetMoveFlag(CaptureTheFlagFlagDirection.Up, speedMultiplier);
                }
                else if (agentCountsAroundFlagChanged)
                {
                    flag.ChangeMovementSpeed(speedMultiplier);
                }
            }
            else if (flag.IsContested)
            {
                flag.SetMoveFlag(CaptureTheFlagFlagDirection.Up, speedMultiplier);
            }
            else if (agentCountsAroundFlagChanged)
            {
                flag.ChangeMovementSpeed(speedMultiplier);
            }

            flag.OnAfterTick(closestAgentToFlag != null, out bool flagOwnerChanged);
            if (flagOwnerChanged)
            {
                Team team = closestAgentToFlag!.Team!;
                flag.SetTeamColorsWithAllSynched(team.Color, team.Color2);
                _flagOwners[flag.FlagIndex] = team;
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new FlagDominationCapturePointMessage(flag.FlagIndex, team));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                _battleClient.CaptureFlag(flag, team);
                _notificationsComponent.FlagXCapturedByTeamX(flag, closestAgentToFlag.Team);
                MPPerkObject.RaiseEventForAllPeers(MPPerkCondition.PerkEventFlags.FlagCapture);
            }
        }
    }

    public int GetNumberOfAttackersAroundFlag(FlagCapturePoint flag)
    {
        var flagOwner = GetFlagOwner(flag);
        if (flagOwner == null)
        {
            return 0;
        }

        int count = 0;
        var proximitySearch = AgentProximityMap.BeginSearch(Mission.Current, flag.Position.AsVec2, FlagCaptureRange);
        for (; proximitySearch.LastFoundAgent != null; AgentProximityMap.FindNext(Mission.Current, ref proximitySearch))
        {
            Agent agent = proximitySearch.LastFoundAgent;
            if (agent.IsHuman
                && agent.IsActive()
                && agent.Position.DistanceSquared(flag.Position) <= FlagCaptureRangeSquared
                && agent.Team.Side != flagOwner.Side)
            {
                count += 1;
            }
        }

        return count;
    }

    /// <summary>Gets one random flag from the map.</summary>
    public abstract FlagCapturePoint GetRandomFlag();

    public abstract void CheckForManipulationOfFlags();

    public bool HasFlagCountChanged() => _hasFlagCountChanged;

    public void SetHasFlagCountChanged(bool hasFlagCountChanged) => _hasFlagCountChanged = hasFlagCountChanged;

    public Timer GetCheckFlagRemovalTimer(float currentTime, float flagManipulationTime)
    {
        if (_checkFlagRemovalTimer != null)
        {
            return _checkFlagRemovalTimer;
        }

        _checkFlagRemovalTimer = new Timer(currentTime, flagManipulationTime);
        return _checkFlagRemovalTimer;
    }

    public void SetCheckFlagRemovalTimer(Timer? checkFlagRemovalTimer) => _checkFlagRemovalTimer = checkFlagRemovalTimer;

    protected CrpgBattleClient GetBattleClient() => _battleClient;

    protected MultiplayerGameNotificationsComponent GetNotificationsComponent() => _notificationsComponent;

    protected abstract bool CanAgentCaptureFlag(Agent agent);

    protected abstract void ResetFlag(FlagCapturePoint flag);

    /// <summary>Checks the flag index are from 0 to N.</summary>
    private void ThrowOnBadFlagIndexes(FlagCapturePoint[] flags)
    {
        int expectedIndex = 0;
        foreach (var flag in flags.OrderBy(f => f.FlagIndex))
        {
            if (flag.FlagIndex != expectedIndex)
            {
                throw new Exception($"Invalid scene '{Mission.Current?.SceneName}': Flag indexes should be numbered from 0 to {flags.Length}");
            }

            expectedIndex += 1;
        }
    }
}
