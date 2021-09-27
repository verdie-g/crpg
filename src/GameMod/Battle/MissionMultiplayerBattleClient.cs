using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod.Battle
{
    public class MissionMultiplayerBattleClient : MissionMultiplayerGameModeBaseClient
    {
        public override bool IsGameModeUsingGold => false;
        public override bool IsGameModeTactical => false;
        public override bool IsGameModeUsingRoundCountdown => true;
        public override MissionLobbyComponent.MultiplayerGameType GameType => (MissionLobbyComponent.MultiplayerGameType)10; // use not existing game type

        public override int GetGoldAmount() => 0;

        public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
        {
        }
    }
}
