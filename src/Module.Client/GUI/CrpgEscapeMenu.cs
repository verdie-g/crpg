using System;
using System.Diagnostics;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Multiplayer;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.PlatformService;

namespace Crpg.Module.GUI;

[OverrideView(typeof(CrpgMissionMultiplayerEscapeMenu))]
public class CrpgEscapeMenu : MissionGauntletMultiplayerEscapeMenu
{
    private const string CrpgWebsite = "https://c-rpg.eu";
    public CrpgEscapeMenu(string gameType)
        : base(gameType)
    {
    }

    protected override List<EscapeMenuItemVM> GetEscapeMenuItems()
    {
        List<EscapeMenuItemVM> list = base.GetEscapeMenuItems();
        var crpgWebsiteButton = new EscapeMenuItemVM(new TextObject("cRPG Website", null),  o =>
        {
            InformationManager.DisplayMessage(new InformationMessage("Opening cRPG website.."));
            ExecuteOpenLink(CrpgWebsite);
        }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false);

        list.Insert(list.Count - 2, crpgWebsiteButton); // -2 = Insert new button right before the 'Option' button

        return list;
    }

    private void ExecuteOpenLink(string url)
    {
        // Try to open the website through steam. If it fails it will use the default webbrowser.
        if (!string.IsNullOrEmpty(url) && !PlatformServices.Instance.ShowOverlayForWebPage(url).Result)
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true,
            });
        }
    }
}
