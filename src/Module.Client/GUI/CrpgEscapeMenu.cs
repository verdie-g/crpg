using System;
using System.Diagnostics;
using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Multiplayer;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.PlatformService;

namespace Crpg.Module.GUI;

// We need to use the OverrideView here since the CrpgEscapeMenu constructor requires a parameter.
[OverrideView(typeof(CrpgMissionMultiplayerEscapeMenu))]
internal class CrpgEscapeMenu : MissionGauntletMultiplayerEscapeMenu
{
    private const string CrpgWebsite = "https://c-rpg.eu";
    private const int BottomMenuOffset = 2; // -2 = Insert new buttons right before the 'Option' button
    private NoTeamSelectComponent? _multiplayerTeamSelectComponent;
    public CrpgEscapeMenu(string gameType)
        : base(gameType)
    {
    }

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();
        _multiplayerTeamSelectComponent = Mission.GetMissionBehavior<NoTeamSelectComponent>();
    }

    protected override List<EscapeMenuItemVM> GetEscapeMenuItems()
    {
        List<EscapeMenuItemVM> items = base.GetEscapeMenuItems();
        EscapeMenuItemVM crpgWebsiteButton = new(new TextObject("Character & Shop", null),  _ =>
        {
            _ = ExecuteOpenCrpgWebsite();
        }, null, () => Tuple.Create(false, TextObject.Empty), false);

        MissionPeer myMissionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
        if (myMissionPeer != null)
        {
            string spectatorButtonLabel = myMissionPeer.Team?.Side != BattleSideEnum.None ? "Join spectators" : "Join the game";
            EscapeMenuItemVM spectatorButton = new(new TextObject(spectatorButtonLabel, null), _ =>
            {
                _multiplayerTeamSelectComponent?.RequestTeamChange(myMissionPeer.Team?.Side == BattleSideEnum.None);
                OnEscapeMenuToggled(false);
            }, null, () => Tuple.Create(false, TextObject.Empty), false);
            items.Insert(items.Count - BottomMenuOffset, spectatorButton);
        }

        items.Insert(items.Count - BottomMenuOffset, crpgWebsiteButton);

        return items;
    }

    private async Task ExecuteOpenCrpgWebsite()
    {
        // Try to open the website through steam. If it fails it will use the default webbrowser.
        if (!await PlatformServices.Instance.ShowOverlayForWebPage(CrpgWebsite))
        {
            Process.Start(new ProcessStartInfo(CrpgWebsite)
            {
                UseShellExecute = true,
            }).Dispose();
            InformationManager.DisplayMessage(new InformationMessage("Please check your webbrowser.."));
        }
    }
}
