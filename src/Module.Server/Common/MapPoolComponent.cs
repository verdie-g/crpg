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
        if (_forcedNextMap != null)
        {
            foreach (var vote in MultiplayerIntermissionVotingManager.Instance.MapVoteItems)
            {
                vote.SetVoteCount(vote.Id == _forcedNextMap ? 1 : 0);
            }

            _forcedNextMap = null;
        }
    }
}
