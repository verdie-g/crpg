using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace Crpg.Module.Modes.Battle.FlagSystems;

internal abstract class FlagSystem
{
#pragma warning disable SA1401 // False negative.
    protected readonly Mission Mission;
#pragma warning restore SA1401

    protected FlagSystem(Mission mission)
    {
        Mission = mission;
    }

    public abstract IEnumerable<FlagCapturePoint> AllCapturePoints { get; }
    public abstract event Action<BattleSideEnum, float>? OnMoraleChangedEvent;
    public abstract event Action? OnFlagNumberChangedEvent;
    public abstract event Action<FlagCapturePoint, Team>? OnCapturePointOwnerChangedEvent;

    public abstract void Reset();
    public abstract void Tick(float dt);
    public abstract Team? GetFlagOwner(FlagCapturePoint flag);
    public abstract bool HasRoundEnded();
    public abstract (BattleSideEnum side, RoundEndReason reason) GetRoundWinner(bool timedOut);
    public abstract bool ShouldOvertime();
    public abstract int GetWarningTimer(float remainingRoundTime, float roundTimeLimit);
    public abstract void HandleNewClient(NetworkCommunicator networkPeer);
    public abstract void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer);
}
