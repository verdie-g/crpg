using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Balancing;

internal class WeightedCrpgUser
{
    public WeightedCrpgUser(CrpgUser user, float weight)
    {
        User = user;
        Weight = weight;
    }

    public CrpgUser User { get; }
    public int? ClanId => User.ClanMembership?.ClanId;
    public float Weight { get; }
}
