using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Crpg.Module.Balancing;
using Crpg.Module.Common;
using JetBrains.Annotations;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Balancing;
internal class UpdateRating : MissionBehavior

{
    UpdateRating(MultiplayerRoundController roundController)
    {
        _roundController = roundController;
    }
    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Logic;
    private readonly MultiplayerRoundController _roundController = new();
    RatingPeriodResults results = new();
    List<RatedUser> ratedUsers = new();
    RatingCalculator calculator = new();
    public void InitializeRatingTools()
    {
        calculator = new RatingCalculator();
        ratedUsers = new List<RatedUser>();
        results = new();

    }
    public override void OnBehaviorInitialize()
    {
        _roundController.OnRoundEnding += UpdateRatingAtRoundEnd;
    }

    public override void OnRemoveBehavior()
    {
    }
    public override void OnAgentCreated(Agent agent)
    {
            var crpgRepresentative = agent.MissionPeer.Peer.GetComponent<CrpgRepresentative>();
            if (crpgRepresentative == null)
            {
            }
            else
            {
                if (!(crpgRepresentative.User == null))
                {
                float characterRating = crpgRepresentative.User.Character.Rating;
                float characterRatingDeviation = crpgRepresentative.User.Character.Rating;
                float characterVolatility = crpgRepresentative.User.Character.Volatility;
                RatedUser ratedUser = new RatedUser(crpgRepresentative.User, calculator, characterRating,characterRatingDeviation,characterVolatility);
                ratedUsers.Add(ratedUser);
                }
                else
                {
                }
            }
    }

    public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
    {
        var crpgRepresentativeWinner = affectorAgent.MissionPeer.Peer.GetComponent<CrpgRepresentative>();
        var crpgRepresentativeLoser = affectedAgent.MissionPeer.Peer.GetComponent<CrpgRepresentative>();
        if (!(crpgRepresentativeWinner == null | crpgRepresentativeLoser == null))
        {
            Rating winner = ratedUsers.Find(r => r.Crpguser == crpgRepresentativeWinner!.User).Rating;
            Rating loser = ratedUsers.Find(r => r.Crpguser == crpgRepresentativeLoser!.User).Rating;
            float percentage = blow.InflictedDamage / affectedAgent.BaseHealthLimit;
            results.AddResult(winner, loser, percentage);
        }
    }

    public void UpdateRatingAtRoundEnd()
    {
        foreach (RatedUser user in ratedUsers)
        {
            calculator.UpdateRatings(results);
            user.Crpguser.Character.Rating = (float) user.Rating.GetRating();
            user.Crpguser.Character.RatingDeviation = (float)user.Rating.GetRatingDeviation();
            user.Crpguser.Character.Volatility = (float)user.Rating.GetVolatility();

        }


    }
}
