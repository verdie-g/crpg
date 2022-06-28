using Crpg.Module.Api.Models.Users;
using TaleWorlds.Core;

namespace Crpg.Module.Common;

internal class CrpgPeer : PeerComponent
{
    public CrpgUser? User { get; set; }
    public int RewardMultiplier { get; set; }
}
