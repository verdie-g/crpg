﻿using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.DefendTheVirgin;

public class MissionCombatantsLogic : MissionLogic
{
    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();

        Mission.Teams.Add(BattleSideEnum.Defender, banner: Banner.CreateRandomBanner());
        Mission.Teams.Add(BattleSideEnum.Attacker, banner: Banner.CreateRandomBanner());
        Mission.PlayerTeam = Mission.DefenderTeam;
    }

    public override void EarlyStart()
    {
        Mission.AttackerTeam.AddTeamAI(new TeamAIGeneral(Mission, Mission.AttackerTeam));
        Mission.AttackerTeam.AddTacticOption(new TacticCharge(Mission.AttackerTeam));
        Mission.AttackerTeam.AddTacticOption(new TacticFullScaleAttack(Mission.AttackerTeam));
        Mission.AttackerTeam.AddTacticOption(new TacticRangedHarrassmentOffensive(Mission.AttackerTeam));
        Mission.AttackerTeam.QuerySystem.Expire();
        Mission.AttackerTeam.ResetTactic();
    }

    public override void AfterStart()
    {
        Mission.SetMissionMode(MissionMode.Battle, true);
    }
}
