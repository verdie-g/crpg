using TaleWorlds.Core;
using TaleWorlds.Library;
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
    private const int MaxMapsToVote = 2; // N.B: Only 5 maps fit in the intermission screen.

    /// <summary>The entire map pool. Needs to be static to survive the mission change.</summary>
    private static string[]? _maps;
    private static int _mapsIndex;
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
        bool firstMission = _maps == null; // _maps is null only when the first mission ends.

        var votingManager = MultiplayerIntermissionVotingManager.Instance;
        var mapVoteItems = votingManager.MapVoteItems;
        if (_maps == null)
        {
            _maps = DedicatedCustomServerSubModule.Instance.AutomatedMapPool.ToArray();

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
                var lastVoteLostMaps = mapVoteItems.Where(vote => vote.Id != Mission.SceneName);
                Debug.Print($"Map {Mission.SceneName} was voted over {string.Join(",", lastVoteLostMaps)}");
            }

            mapVoteItems.Clear();
            int maxMapsToVote = Math.Min(_mapsIndex + MaxMapsToVote, _maps.Length);
            for (; _mapsIndex < maxMapsToVote; _mapsIndex += 1)
            {
                mapVoteItems.Add(new IntermissionVoteItem(_maps[_mapsIndex], 0));
                // mapVoteItems[_maps[_mapsIndex]] = 0;
            }

            // Vote result is ignored is there is only one map, so we need to force it here.
            if (mapVoteItems.Count == 1)
            {
                MultiplayerOptions.Instance
                    .GetOptionFromOptionType(MultiplayerOptions.OptionType.Map)
                    .UpdateValue(mapVoteItems.First().Id);
            }

            if (_mapsIndex >= _maps.Length)
            {
                _mapsIndex = 0;
                _maps.Shuffle();
            }
        }
        else if (_forcedNextMap != null)
        {
            MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.Map).UpdateValue(_forcedNextMap);
            _forcedNextMap = null;
        }
        else
        {
            MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.Map).UpdateValue(_maps[_mapsIndex]);
            _mapsIndex = (_mapsIndex + 1) % _maps.Length;
        }
    }
}
