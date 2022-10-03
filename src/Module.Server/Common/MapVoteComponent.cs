using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/// <summary>
/// Picks X maps randomly from the pool for the <see cref="MultiplayerIntermissionVotingManager"/>.
/// </summary>
/// <remarks>
/// I could not find a way to branch to game start/end so I branched to <see cref="OnEndMission"/>. It means that the
/// intermission screen when the game start, will show all maps. It's ok since usually nobody is connected when the
/// server starts.
/// </remarks>
internal class MapVoteComponent : MissionBehavior
{
    private const int MaxMapsToVote = 5; // Only 5 maps fit in the intermission screen.

    private string[]? _maps;

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    protected override void OnEndMission()
    {
        var mapVoteItems = MultiplayerIntermissionVotingManager.Instance.MapVoteItems;
        _maps ??= mapVoteItems.Keys.ToArray(); // _maps is null only when the first mission ends.

        _maps.Shuffle();
        mapVoteItems.Clear();
        for (int i = 0; i < Math.Min(_maps.Length, MaxMapsToVote); i += 1)
        {
            mapVoteItems[_maps[i]] = 0;
        }
    }
}
