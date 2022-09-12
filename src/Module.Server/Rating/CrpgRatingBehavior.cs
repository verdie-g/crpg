using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Rating;

internal class CrpgRatingBehavior : MissionBehavior
{
    private readonly Dictionary<PlayerId, CrpgRatedUser> _ratedUsers;
    private readonly RatingCalculator _calculator;
    private RatingPeriodResults _results;

    public CrpgRatingBehavior()
    {
        _ratedUsers = new Dictionary<PlayerId, CrpgRatedUser>();
        _calculator = new RatingCalculator();
        _results = new RatingPeriodResults();
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Logic;

    public override void OnAgentHit(
        Agent affectedAgent,
        Agent affectorAgent,
        in MissionWeapon affectorWeapon,
        in Blow blow,
        in AttackCollisionData attackCollisionData)
    {
        if (!TryGetRatedUser(affectorAgent, out var ratedAffector)
            || !TryGetRatedUser(affectedAgent, out var ratedAffected))
        {
            return;
        }

        // TODO: self hit
        // TODO: team hit
        float inflictedRatio = blow.InflictedDamage / affectedAgent.BaseHealthLimit;
        _results.AddResult(ratedAffector.Rating, ratedAffected.Rating, inflictedRatio);
    }

    public Dictionary<PlayerId, CrpgCharacterRating> ComputeRatingsAndReset()
    {
        Dictionary<PlayerId, CrpgCharacterRating> ratings = new(_ratedUsers.Count);
        foreach (var user in _ratedUsers)
        {
            _calculator.UpdateRatings(_results);
            // user.Character.Character.Rating = (float) user.Rating.GetRating();
            // user.Character.Character.RatingDeviation = (float)user.Rating.GetRatingDeviation();
            // user.Character.Character.Volatility = (float)user.Rating.GetVolatility();
        }

        _results = new RatingPeriodResults();
        _ratedUsers.Clear();

        return ratings;
    }

    private bool TryGetRatedUser(Agent agent, out CrpgRatedUser ratedUser)
    {
        ratedUser = null!;
        if (agent.MissionPeer == null)
        {
            return false;
        }

        if (!_ratedUsers.TryGetValue(agent.MissionPeer.Peer.Id, out ratedUser))
        {
            var crpgRepresentative = agent.MissionPeer?.Peer.GetComponent<CrpgRepresentative>();
            if (crpgRepresentative == null)
            {
                return false;
            }

            ratedUser = new CrpgRatedUser(crpgRepresentative, _calculator);
            _ratedUsers[crpgRepresentative.MissionPeer.Peer.Id] = ratedUser;
        }

        return true;
    }
}
