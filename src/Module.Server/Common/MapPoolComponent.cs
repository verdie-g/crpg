using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/// <summary>
/// If voting is enabled, allow voting for the X next maps of the pool; otherwise shuffle once the pool and picks the maps sequentially.
/// </summary>
/// <remarks>
/// I could not find a way to branch to game start/end so I branched to <see cref="OnEndMission"/>. It means that the
/// intermission screen when the game start, will show all maps. It's ok since usually nobody is connected when the
/// server starts.
/// </remarks>
internal class MapPoolComponent : MissionBehavior
{
    private const int MaxMapsToVote = 2; // N.B: Only 5 maps fit in the intermission screen.

    /// <summary>The entire map pool. Needs to be static to survive the mission change.</summary>
    private static string[]? _maps;
    private static int _mapsIndex;

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    protected override void OnEndMission()
    {
        bool firstMission = _maps == null; // _maps is null only when the first mission ends.

        var votingManager = MultiplayerIntermissionVotingManager.Instance;
        var mapVoteItems = votingManager.MapVoteItems;
        if (_maps == null)
        {
            _maps = mapVoteItems.Keys.ToArray();
            mapVoteItems.Clear();

            _maps.Shuffle();
            // Move back the first map in first position.
            int currentMapIndex = Array.IndexOf(_maps, Mission.SceneName);
            (_maps[currentMapIndex], _maps[0]) = (_maps[0], _maps[currentMapIndex]);
            _mapsIndex = 1;
        }

        if (votingManager.IsMapVoteEnabled)
        {
            if (!firstMission && GameNetwork.NetworkPeers.Count() > 10)
            {
                var lastVoteLostMaps = mapVoteItems.Keys.Where(m => m != Mission.SceneName);
                Debug.Print($"Map {Mission.SceneName} was voted over {string.Join(",", lastVoteLostMaps)}");
            }

            mapVoteItems.Clear();
            int maxMapsToVote = Math.Min(_mapsIndex + MaxMapsToVote, _maps.Length);
            for (; _mapsIndex < maxMapsToVote; _mapsIndex += 1)
            {
                mapVoteItems[_maps[_mapsIndex]] = 0;
            }

            if (_mapsIndex >= _maps.Length)
            {
                _mapsIndex = 0;
                _maps.Shuffle();
            }
        }
        else
        {
            MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.Map).UpdateValue(_maps[_mapsIndex]);
            _mapsIndex = (_mapsIndex + 1) % _maps.Length;
        }
    }
}
