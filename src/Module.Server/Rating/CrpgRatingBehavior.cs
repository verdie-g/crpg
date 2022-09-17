using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Rating;

internal class CrpgRatingBehavior : MissionBehavior
{
    private readonly Dictionary<PlayerId, CrpgRating> _playerRatings;
    private readonly CrpgRatingPeriodResults _results;

    public CrpgRatingBehavior()
    {
        _playerRatings = new Dictionary<PlayerId, CrpgRating>();
        _results = new CrpgRatingPeriodResults();
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void OnAgentHit(
        Agent affectedAgent,
        Agent affectorAgent,
        in MissionWeapon affectorWeapon,
        in Blow blow,
        in AttackCollisionData attackCollisionData)
    {
        if (!TryGetRating(affectorAgent, out var affectorRating)
            || !TryGetRating(affectedAgent, out var affectedRating))
        {
            return;
        }

        // TODO: self hit
        // TODO: team hit
        float inflictedRatio = blow.InflictedDamage / affectedAgent.BaseHealthLimit;
        _results.AddResult(affectorRating, affectedRating, inflictedRatio);
    }

    public Dictionary<PlayerId, CrpgCharacterRating> ComputeRatingsAndReset()
    {
        CrpgRatingCalculator.UpdateRatings(_results);
        Dictionary<PlayerId, CrpgCharacterRating> characterRatings = new(_playerRatings.Count);
        foreach (var playerRating in _playerRatings)
        {
            characterRatings[playerRating.Key] = new CrpgCharacterRating
            {
                Value = (float)playerRating.Value.Glicko2Rating,
                Deviation = (float)playerRating.Value.Glicko2RatingDeviation,
                Volatility = (float)playerRating.Value.Volatility,
            };
        }

        _results.Clear();
        _playerRatings.Clear();

        return characterRatings;
    }

    private bool TryGetRating(Agent agent, out CrpgRating rating)
    {
        rating = null!;
        if (agent.MissionPeer == null)
        {
            return false;
        }

        if (!_playerRatings.TryGetValue(agent.MissionPeer.Peer.Id, out rating))
        {
            var crpgRepresentative = agent.MissionPeer?.Peer.GetComponent<CrpgRepresentative>();
            if (crpgRepresentative == null)
            {
                return false;
            }

            var characterRating = crpgRepresentative.User!.Character.Rating;
            rating = new(characterRating.Value, characterRating.Deviation, characterRating.Volatility);
            _playerRatings[crpgRepresentative.MissionPeer.Peer.Id] = rating;
            _results.AddParticipant(rating);
        }

        return true;
    }
}
