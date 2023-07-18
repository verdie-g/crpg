using Crpg.Module.Api.Models.Items;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class SpawnInfo
{
    public SpawnInfo(Team team, IList<CrpgEquippedItem> equippedItems)
    {
        Team = team;
        EquippedItems = equippedItems;
    }

    /// <summary>The team the user has spawn in. Used to give the correct reward multiplier even after changing team.</summary>
    public Team Team { get; }

    /// <summary>The equipment the user has spawn in. Used to pay the correct upkeep.</summary>
    public IList<CrpgEquippedItem> EquippedItems { get; }
}
