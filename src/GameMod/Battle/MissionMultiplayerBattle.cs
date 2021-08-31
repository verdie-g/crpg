using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.GameMod.Battle
{
    public class MissionMultiplayerBattle : MissionMultiplayerGameModeBase
    {
        public override bool IsGameModeHidingAllAgentVisuals => false;
        public override bool IsGameModeUsingOpposingTeams => true;

        public override MissionLobbyComponent.MultiplayerGameType GetMissionType()
        {
            return (MissionLobbyComponent.MultiplayerGameType)10;
        }

        public override void AfterStart()
        {
            base.AfterStart();
            InitializeMissionTeams();
        }

        /// <summary>
        /// Warm-up ends if the maximum number of player on the server was reached.
        /// </summary>
        public override bool CheckForWarmupEnd()
        {
            int playersInTeams = 0;
            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                if (networkPeer.IsSynchronized && component.Team != null && component.Team.Side != BattleSideEnum.None)
                {
                    playersInTeams += 1;
                }
            }

            return playersInTeams >= MultiplayerOptions.OptionType.MaxNumberOfPlayers.GetIntValue();
        }

        private void InitializeMissionTeams()
        {
          var cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
          Banner banner1 = new(cultureTeam1.BannerKey, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1);
          Mission.Teams.Add(BattleSideEnum.Attacker, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1, banner1, false);

          var cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
          Banner banner2 = new(cultureTeam2.BannerKey, cultureTeam2.BackgroundColor2,
              cultureTeam2.ForegroundColor2);
          Mission.Teams.Add(BattleSideEnum.Defender, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2, banner2, false);
        }
    }
}
