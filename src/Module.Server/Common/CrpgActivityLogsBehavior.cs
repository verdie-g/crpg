using Crpg.Module.Api;
using Crpg.Module.Api.Models.ActivityLogs;
using Crpg.Module.Common.Warmup;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgActivityLogsBehavior : MissionLogic
{
    private const int LogsBufferSize = 100;

    private readonly CrpgWarmupComponent? _warmupComponent;
    private readonly ChatBox _chatBox;
    private readonly ICrpgClient _crpgClient;
    private readonly List<CrpgActivityLog> _logsBuffer;

    public CrpgActivityLogsBehavior(CrpgWarmupComponent? warmupComponent, ChatBox chatBox, ICrpgClient crpgClient)
    {
        _warmupComponent = warmupComponent;
        _chatBox = chatBox;
        _crpgClient = crpgClient;
        _logsBuffer = new List<CrpgActivityLog>();
    }

    public override void OnBehaviorInitialize()
    {
        _chatBox.OnMessageReceivedAtDedicatedServer = (Action<NetworkCommunicator, string>)Delegate.Combine(
            OnMessageReceivedAtDedicatedServer,
            _chatBox.OnMessageReceivedAtDedicatedServer);
    }

    public override void OnRemoveBehavior()
    {
        _chatBox.OnMessageReceivedAtDedicatedServer = (Action<NetworkCommunicator, string>)Delegate.Remove(
            _chatBox.OnMessageReceivedAtDedicatedServer,
            OnMessageReceivedAtDedicatedServer)!;

        FlushLogs();
    }

    public override void OnScoreHit(
        Agent affectedAgent,
        Agent? affectorAgent,
        WeaponComponentData attackerWeapon,
        bool isBlocked,
        bool isSiegeEngineHit,
        in Blow blow,
        in AttackCollisionData collisionData,
        float damagedHp,
        float hitDistance,
        float shotDifficulty)
    {
        if (_warmupComponent is { IsInWarmup: true })
        {
            return;
        }

        int? affectedUserId = affectedAgent?.MissionPeer?.GetComponent<CrpgPeer>()?.User?.Id;
        int? affectorUserId = affectorAgent?.MissionPeer?.GetComponent<CrpgPeer>()?.User?.Id;
        if (affectedUserId == null || affectorUserId == null)
        {
            return;
        }

        if (affectorAgent!.Team != null
            && affectorAgent.Team.Side != BattleSideEnum.None
            && affectorAgent.Team == affectedAgent!.Team)
        {
            AddTeamHitLog(affectorUserId.Value, affectedUserId.Value, (int)damagedHp);
        }
    }

    private void OnMessageReceivedAtDedicatedServer(NetworkCommunicator fromPeer, string message)
    {
        int? userId = fromPeer.GetComponent<CrpgPeer>()?.User?.Id;
        if (userId == null)
        {
            return;
        }

        AddChatMessageSentLog(userId.Value, message);
    }

    // TODO
    private void AddServerJoinedLog(int userId)
    {
        AddLog(CrpgActivityLogType.ServerJoined, userId, new Dictionary<string, string>());
    }

    private void AddChatMessageSentLog(int userId, string message)
    {
        AddLog(CrpgActivityLogType.ChatMessageSent, userId, new Dictionary<string, string>
        {
            ["message"] = message,
        });
    }

    private void AddTeamHitLog(int userId, int targetUserId, int damage)
    {
        AddLog(CrpgActivityLogType.TeamHit, userId, new Dictionary<string, string>
        {
            ["targetUserId"] = targetUserId.ToString(),
            ["damage"] = damage.ToString(),
        });
    }

    private void AddLog(CrpgActivityLogType type, int userId, Dictionary<string, string> metadata)
    {
        CrpgActivityLog log = new()
        {
            Type = type,
            UserId = userId,
            Metadata = new Dictionary<string, string>(metadata) { ["instance"] = CrpgServerConfiguration.Instance },
            CreatedAt = DateTime.UtcNow,
        };

        _logsBuffer.Add(log);
        if (_logsBuffer.Count >= LogsBufferSize)
        {
            FlushLogs();
        }
    }

    private void FlushLogs()
    {
        if (_logsBuffer.Count == 0)
        {
            return;
        }

        _ = _crpgClient.CreateActivityLogsAsync(_logsBuffer.ToArray());
        Debug.Print($"Sent {_logsBuffer.Count} activity logs");
        _logsBuffer.Clear();
    }
}
