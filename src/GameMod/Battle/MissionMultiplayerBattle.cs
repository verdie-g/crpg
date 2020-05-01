using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod.Battle
{
    public class MissionMultiplayerBattle : MissionMultiplayerGameModeBase
    {
        public override bool IsGameModeHidingAllAgentVisuals => false;

        public override MissionLobbyComponent.MultiplayerGameType GetMissionType()
        {
            return (MissionLobbyComponent.MultiplayerGameType)10;
        }

        public override void AfterStart()
        {
            base.AfterStart();
            InitializeMissionTeams();
        }

        private void InitializeMissionTeams()
        {
          var cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
          Banner banner1 = new Banner(cultureTeam1.BannerKey, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1);
          Mission.Teams.Add(BattleSideEnum.Attacker, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1, banner1, false);

          var cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
          Banner banner2 = new Banner(cultureTeam2.BannerKey, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2);
          Mission.Teams.Add(BattleSideEnum.Defender, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2, banner2, false);
        }
    }
}