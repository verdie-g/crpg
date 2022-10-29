﻿using System;
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
    private readonly string _gameType;
    public CrpgEscapeMenu(string gameType)
        : base(gameType)
    {
        _gameType = gameType;
    }

    protected override List<EscapeMenuItemVM> GetEscapeMenuItems()
    {
        List<EscapeMenuItemVM> items = base.GetEscapeMenuItems();
        EscapeMenuItemVM crpgWebsiteButton = new(new TextObject("Character & Shop", null),  _ =>
        {
            _ = ExecuteOpenCrpgWebsite();
        }, null, () => Tuple.Create(false, TextObject.Empty), false);

        if (_gameType == "Duel")
        {
            items.RemoveRange(2, 2); // Remove 'Change Culture' and 'Change troop' button
            AddDuelModeOptions(items);
        }

        items.Insert(items.Count - 1, crpgWebsiteButton); // -1 = Insert new button right before the 'Quit' button

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

        EscapeMenuItemVM preferedArenaInfButton = new(new TextObject("Arena: Infantry", null), _ =>
        {
            DuelModeChangeArena(TroopType.Infantry);
            OnEscapeMenuToggled(false);
        }, null, () => Tuple.Create(false, TextObject.Empty), false);

        EscapeMenuItemVM preferedArenaArcButton = new(new TextObject("Arena: Ranged", null), _ =>
        {
            DuelModeChangeArena(TroopType.Ranged);
            OnEscapeMenuToggled(false);
        }, null, () => Tuple.Create(false, TextObject.Empty), false);

        EscapeMenuItemVM preferedArenaCavButton = new(new TextObject("Arena: Cavalry", null), _ =>
        {
            DuelModeChangeArena(TroopType.Cavalry);
            OnEscapeMenuToggled(false);
        }, null, () => Tuple.Create(false, TextObject.Empty), false);

        List<EscapeMenuItemVM> newButtons = new() { preferedArenaInfButton, preferedArenaArcButton, preferedArenaCavButton };
        items.InsertRange(items.Count - 1, newButtons);
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
