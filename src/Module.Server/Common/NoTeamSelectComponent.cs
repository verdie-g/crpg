using Crpg.Module.Battle;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/// <summary>
/// Disables team selection. Auto select is done in <see cref="CrpgFlagDominationMissionMultiplayer.HandleLateNewClientAfterSynchronized"/>.
/// </summary>
internal class NoTeamSelectComponent : MultiplayerTeamSelectComponent
{
    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        typeof(MultiplayerTeamSelectComponent).GetProperty("TeamSelectionEnabled")!.SetValue(this, false);
    }
}
