using Crpg.Module.Common.Network;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;
using Timer = TaleWorlds.Core.Timer;

namespace Crpg.Module.Common;

/// <summary>
/// Basically a copy of <see cref="MissionPeer.TickInactivityStatus"/>.
/// </summary>
internal class KickInactiveBehavior : MissionBehavior
{
    private readonly MissionTime _inactiveTimeLimit;
    private readonly MultiplayerWarmupComponent _warmupComponent;
    private readonly Dictionary<PlayerId, ActivityStatus> _lastActiveStatuses;
    private Timer? _checkTimer;

    public KickInactiveBehavior(
        float inactiveTimeLimit,
        MultiplayerWarmupComponent warmupComponent)
    {
        _inactiveTimeLimit = MissionTime.Seconds(inactiveTimeLimit);
        _warmupComponent = warmupComponent;
        _lastActiveStatuses = new Dictionary<PlayerId, ActivityStatus>();
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void OnMissionTick(float dt)
    {
        if (_warmupComponent.IsInWarmup)
        {
            return;
        }

        _checkTimer ??= new Timer(Mission.CurrentTime, 1f);
        if (!_checkTimer.Check(Mission.CurrentTime))
        {
            return;
        }

        foreach (var networkPeer in GameNetwork.NetworkPeers)
        {
            var playerId = networkPeer.VirtualPlayer.Id;
            var agent = networkPeer.ControlledAgent;
            if (agent == null || !agent.IsActive())
            {
                _lastActiveStatuses.Remove(playerId);
                continue;
            }

            if (!_lastActiveStatuses.TryGetValue(playerId, out var lastActiveStatus))
            {
                _lastActiveStatuses[playerId] = new ActivityStatus
                {
                    LastActive = MissionTime.Now,
                    MovementFlags = agent.MovementFlags,
                    MovementInputVector = agent.MovementInputVector,
                    LookDirection = agent.LookDirection,
                    Warned = false,
                };
                continue;
            }

            ActivityStatus newActiveStatus = new()
            {
                LastActive = MissionTime.Now,
                MovementFlags = agent.MovementFlags,
                MovementInputVector = agent.MovementInputVector,
                LookDirection = agent.LookDirection,
                IsUsingGameObject = agent.IsUsingGameObject,
                Warned = false,
            };

            if (newActiveStatus.IsUsingGameObject
                || lastActiveStatus.MovementFlags != newActiveStatus.MovementFlags
                || lastActiveStatus.MovementInputVector.DistanceSquared(newActiveStatus.MovementInputVector) > 1E-05f
                || lastActiveStatus.LookDirection.DistanceSquared(newActiveStatus.LookDirection) > 1E-05f)
            {
                _lastActiveStatuses[playerId] = newActiveStatus;
                continue;
            }

            if (MissionTime.Now - lastActiveStatus.LastActive > _inactiveTimeLimit)
            {
                var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
                Debug.Print($"Kick inactive user {crpgPeer.User!.Character.Name} ({crpgPeer.User.Platform}#{crpgPeer.User.PlatformUserId})");
                KickHelper.Kick(networkPeer, DisconnectType.Inactivity);
                return;
            }

            const float warningTime = 15;
            if (MissionTime.Now - lastActiveStatus.LastActive > _inactiveTimeLimit - MissionTime.Seconds(warningTime) && !lastActiveStatus.Warned)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new CrpgNotification
                {
                    Type = CrpgNotificationType.Notification,
                    Message = $"You will be removed for inactivity after {warningTime} seconds!",
                    SoundEvent = string.Empty,
                });
                GameNetwork.EndModuleEventAsServer();

                lastActiveStatus.Warned = true;
                _lastActiveStatuses[playerId] = lastActiveStatus;
                return;
            }
        }
    }

    private struct ActivityStatus
    {
        public MissionTime LastActive { get; set; }
        public Agent.MovementControlFlag MovementFlags { get; set; }
        public Vec2 MovementInputVector { get; set; }
        public Vec3 LookDirection { get; set; }
        public bool IsUsingGameObject { get; set; }
        public bool Warned { get; set; }
    }
}
