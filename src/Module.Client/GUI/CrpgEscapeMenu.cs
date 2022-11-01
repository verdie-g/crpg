using System;
using System.Diagnostics;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Multiplayer;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.PlatformService;

namespace Crpg.Module.GUI;

// We need to use the OverrideView here since the CrpgEscapeMenu constructor requires a parameter.
[OverrideView(typeof(CrpgMissionMultiplayerEscapeMenu))]
internal class CrpgEscapeMenu : MissionGauntletMultiplayerEscapeMenu
{
    private const string CrpgWebsite = "https://c-rpg.eu";
    private readonly bool _isDuelGamemode;
    public CrpgEscapeMenu(string gameType)
        : base(gameType)
    {
        _isDuelGamemode = gameType == "Duel";
    }

    protected override List<EscapeMenuItemVM> GetEscapeMenuItems()
    {
        List<EscapeMenuItemVM> items = base.GetEscapeMenuItems();
        EscapeMenuItemVM crpgWebsiteButton = new(new TextObject("Character & Shop", null),  _ =>
        {
            _ = ExecuteOpenCrpgWebsite();
        }, null, () => Tuple.Create(false, TextObject.Empty), false);

        if (_isDuelGamemode)
        {
            AddDuelModeOptions(items);
        }

        items.Insert(items.Count - 2, crpgWebsiteButton); // -2 = Insert new button right before the 'Options' button

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

    private void AddDuelModeOptions(List<EscapeMenuItemVM> items)
    {
        MissionPeer component = GameNetwork.MyPeer.GetComponent<MissionPeer>();

        if (component == null || component.Team == null || component.Representative is not DuelMissionRepresentative)
        {
            return;
        }

        var duelBehavior = Mission.GetMissionBehavior<MissionMultiplayerGameModeDuelClient>();
        if (duelBehavior?.IsInDuel ?? false)
        {
            return;
        }

        EscapeMenuItemVM preferredArenaInfButton = new(new TextObject("Arena: Infantry", null), _ =>
        {
            DuelModeChangeArena(TroopType.Infantry);
            OnEscapeMenuToggled(false);
        }, null, () => Tuple.Create(false, TextObject.Empty), false);

        EscapeMenuItemVM preferredArenaArcButton = new(new TextObject("Arena: Ranged", null), _ =>
        {
            DuelModeChangeArena(TroopType.Ranged);
            OnEscapeMenuToggled(false);
        }, null, () => Tuple.Create(false, TextObject.Empty), false);

        EscapeMenuItemVM preferredArenaCavButton = new(new TextObject("Arena: Cavalry", null), _ =>
        {
            DuelModeChangeArena(TroopType.Cavalry);
            OnEscapeMenuToggled(false);
        }, null, () => Tuple.Create(false, TextObject.Empty), false);

        List<EscapeMenuItemVM> newButtons = new() { preferredArenaInfButton, preferredArenaArcButton, preferredArenaCavButton };
        items.InsertRange(items.Count - 2, newButtons);
    }

    private void DuelModeChangeArena(TroopType troopType)
    {
        MissionPeer component = GameNetwork.MyPeer.GetComponent<MissionPeer>();
        if (component == null || component.Team == null || component.Representative is not DuelMissionRepresentative)
        {
            return;
        }

        if (component.Team.IsDefender)
        {
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=v5EqMSlD}Can't change arena preference while in duel.", null).ToString()));
            return;
        }

        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new RequestChangePreferredTroopType(troopType));
        GameNetwork.EndModuleEventAsClient();
        Action<TroopType> onMyPreferredZoneChanged = ((DuelMissionRepresentative)component.Representative).OnMyPreferredZoneChanged;
        if (onMyPreferredZoneChanged == null)
        {
            return;
        }

        onMyPreferredZoneChanged(troopType);
    }
}
