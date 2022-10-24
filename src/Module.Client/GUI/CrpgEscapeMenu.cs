using System;
using System.Diagnostics;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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
    public CrpgEscapeMenu(string gameType)
        : base(gameType)
    {
    }

    protected override List<EscapeMenuItemVM> GetEscapeMenuItems()
    {
        List<EscapeMenuItemVM> items = base.GetEscapeMenuItems();
        EscapeMenuItemVM crpgWebsiteButton = new(new TextObject("Character & Shop", null),  _ =>
        {
            _ = ExecuteOpenCrpgWebsite();
        }, null, () => Tuple.Create(false, TextObject.Empty), false);

        items.Insert(items.Count - 2, crpgWebsiteButton); // -2 = Insert new button right before the 'Option' button

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
