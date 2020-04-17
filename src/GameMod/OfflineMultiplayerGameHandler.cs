using TaleWorlds.Core;

namespace Crpg.GameMod
{
    internal class OfflineMultiplayerGameHandler : GameHandler
    {
        protected override void OnEarlyPlayerConnect(VirtualPlayer peer)
        {
            base.OnEarlyPlayerConnect(peer);
            InformationManager.DisplayMessage(new InformationMessage("OnEarlyPlayerConnect"));
        }

        protected override void OnPlayerConnect(VirtualPlayer peer)
        {
            base.OnPlayerConnect(peer);
            InformationManager.DisplayMessage(new InformationMessage("OnPlayerConnect "));
        }

        protected override void OnPlayerDisconnect(VirtualPlayer peer)
        {
            base.OnPlayerDisconnect(peer);
            InformationManager.DisplayMessage(new InformationMessage("OnPlayerDisconnect "));
        }

        public override void OnBeforeSave()
        {
        }

        public override void OnAfterSave()
        {
        }

        protected override void OnTick()
        {
            base.OnTick();
/*InformationManager.DisplayMessage(new InformationMessage("OnTick "));


bool flag = NetworkMain.GameClient.IsInGame && (NetworkMain.GameClient.IsHostingCustomGame || Input.IsKeyDown(InputKey.F12)) && Mission.Current != null && Mission.Current.IsLoadingFinished && !Mission.Current.HasMissionBehaviour<CombatTestMissionController>();
if (flag)
{
Mission.Current.AddMissionBehaviour(new CombatTestMissionController());
}*/
        }
    }
}
