using Crpg.Module.Helpers;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;

namespace Crpg.Module.Common;

/// <summary>
/// If voting is enabled, allow voting for the X next maps of the pool; otherwise shuffle once the pool and picks the maps sequentially.
/// </summary>
/// <remarks>
/// I could not find a way to branch to game start/end so I branched to <see cref="OnEndMission"/>. It means that the
/// intermission screen when the game start, will show all maps. It's ok since usually nobody is connected when the
/// server starts.
/// </remarks>
internal class MapPoolComponent : MissionLogic
{
    private static int _mapVoteItemsIdx;

    private string? _forcedNextMap;

    public void ForceNextMap(string map)
    {
        if (!DedicatedCustomServerSubModule.Instance.AutomatedMapPool.Contains(map))
        {
            return;
        }

        _forcedNextMap = map;
    }

    protected override void OnEndMission()
    {
        // Gotha's fix to cope with the retarded 10 games limit.
        var baseNetworkComponent = GameNetwork.GetNetworkComponent<BaseNetworkComponent>();
        if (baseNetworkComponent != null && baseNetworkComponent.CurrentBattleIndex > 2)
        {
            ReflectionHelper.SetField(DedicatedCustomServerSubModule.Instance, "_currentAutomatedBattleIndex", 1);
            baseNetworkComponent.UpdateCurrentBattleIndex(1);
        }

        var votingManager = MultiplayerIntermissionVotingManager.Instance;
        if (votingManager.MapVoteItems.Count == 0) // When automated_battle_pool is not used.
        {
            return;
        }

        _mapVoteItemsIdx = (_mapVoteItemsIdx + 1) % votingManager.MapVoteItems.Count;

        string nextMap = _forcedNextMap ?? votingManager.MapVoteItems[_mapVoteItemsIdx].Id;
        foreach (var vote in votingManager.MapVoteItems)
        {
            vote.SetVoteCount(vote.Id == nextMap ? 1 : 0);
        }

        _forcedNextMap = null;
    }
}
