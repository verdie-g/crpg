using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using Timer = TaleWorlds.Core.Timer;

namespace Crpg.Module.Modes.Battle.FlagSystems;
internal class CrpgSkirmishFlagSystem : AbstractFlagSystem
{
    public CrpgSkirmishFlagSystem(Mission mission, MultiplayerGameNotificationsComponent notificationsComponent, CrpgBattleClient battleClient)
        : base(mission, notificationsComponent, battleClient)
    {
    }

    public override void CheckForManipulationOfFlags()
    {
        if (HasFlagCountChanged())
        {
            return;
        }

        Timer checkFlagRemovalTimer = GetCheckFlagRemovalTimer(Mission.CurrentTime, GetBattleClient().FlagManipulationTime);
        if (!checkFlagRemovalTimer.Check(Mission.CurrentTime))
        {
            return;
        }

        var randomFlag = GetRandomFlag();

        int[] flagIndexesToRemove = GetAllFlags()
            .Where(f => f.FlagIndex != randomFlag.FlagIndex)
            .Select(RemoveFlag)
            .ToArray();

        SetHasFlagCountChanged(true);

        if (flagIndexesToRemove.Length > 0) // In case there is only one flag on the map.
        {
            GetNotificationsComponent().FlagXRemaining(randomFlag);

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new FlagDominationFlagsRemovedMessage());
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

            GetBattleClient().ChangeNumberOfFlags();
            Debug.Print("Flags were removed");
        }
    }

    public override FlagCapturePoint GetRandomFlag()
    {
        var uncapturedFlags = GetAllFlags().Where(f => GetFlagOwner(f) == null).ToArray();
        var defenderFlags = GetAllFlags().Where(f => GetFlagOwner(f)?.Side == BattleSideEnum.Defender).ToArray();
        var attackerFlags = GetAllFlags().Where(f => GetFlagOwner(f)?.Side == BattleSideEnum.Attacker).ToArray();

        if (uncapturedFlags.Length == GetAllFlags().Length)
        {
            Debug.Print("Last flag is a random uncaptured one");
            return uncapturedFlags.GetRandomElement();
        }

        if (defenderFlags.Length == attackerFlags.Length)
        {
            if (uncapturedFlags.Length != 0)
            {
                Debug.Print("Last flag is a random uncaptured one");
                return uncapturedFlags.GetRandomElement();
            }

            Debug.Print("Last flag is a random captured one");
            return GetAllFlags().GetRandomElement();
        }

        var dominatingTeamFlags = defenderFlags.Length > attackerFlags.Length ? defenderFlags : attackerFlags;

        var contestedFlags = dominatingTeamFlags.Where(f => GetNumberOfAttackersAroundFlag(f) > 0).ToArray();
        if (contestedFlags.Length > 0)
        {
            Debug.Print("Last flag is a contested one of the dominating team");
            return contestedFlags.GetRandomElement();
        }

        Debug.Print("Last flag is a random one of the dominating team");
        return dominatingTeamFlags.GetRandomElement();
    }

    protected override bool CanAgentCaptureFlag(Agent agent) => !agent.IsActive() || !agent.IsHuman;

    protected override void ResetFlag(FlagCapturePoint flag) => flag.ResetPointAsServer(TeammateColorsExtensions.NEUTRAL_COLOR, TeammateColorsExtensions.NEUTRAL_COLOR2);

    private int RemoveFlag(FlagCapturePoint flag)
    {
        flag.RemovePointAsServer();
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new FlagDominationCapturePointMessage(flag.FlagIndex, null));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        return flag.FlagIndex;
    }
}
