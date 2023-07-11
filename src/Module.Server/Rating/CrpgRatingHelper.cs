using Crpg.Module.Api.Models;
using Crpg.Module.Common;

namespace Crpg.Module.Rating;

internal static class CrpgRatingHelper
{
    /// <summary>
    /// Some players play in other servers than their local ones, especially during happy hours. Since they latency
    /// is terrible, their level is too and it skews the balancing results. So we apply a penalty to players not playing
    /// locally here.
    /// </summary>
    public static float ComputeRegionRatingPenalty(CrpgRegion userRegion)
    {
        CrpgRegion serverRegion = CrpgServerConfiguration.Region;

        if (userRegion == serverRegion)
        {
            return 1.0f;
        }

        return (userRegion, serverRegion) switch
        {
            (CrpgRegion.Eu, CrpgRegion.Na) => 0.8f,
            (CrpgRegion.Na, CrpgRegion.Eu) => 0.8f,

            (CrpgRegion.Eu, CrpgRegion.As) => 0.3f,
            (CrpgRegion.As, CrpgRegion.Eu) => 0.3f,

            (CrpgRegion.Eu, CrpgRegion.Oc) => 0.4f,
            (CrpgRegion.Oc, CrpgRegion.Eu) => 0.4f,

            (CrpgRegion.Na, CrpgRegion.As) => 0.5f,
            (CrpgRegion.As, CrpgRegion.Na) => 0.5f,

            (CrpgRegion.Na, CrpgRegion.Oc) => 0.5f,
            (CrpgRegion.Oc, CrpgRegion.Na) => 0.5f,

            (CrpgRegion.As, CrpgRegion.Oc) => 0.7f,
            (CrpgRegion.Oc, CrpgRegion.As) => 0.7f,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
