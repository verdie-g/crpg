using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Models;

/// <summary>
/// Used to adjust raw dmg calculations.
/// </summary>
internal class CrpgStrikeMagnitudeModel : MultiplayerStrikeMagnitudeModel
{
    private readonly CrpgConstants _constants;

    public CrpgStrikeMagnitudeModel(CrpgConstants constants)
    {
        _constants = constants;
    }

    public override float GetBluntDamageFactorByDamageType(DamageTypes damageType)
    {
        float result = 0f;
        switch (damageType)
        {
            case DamageTypes.Blunt:
                result = 1f; // Native 1f
                break;
            case DamageTypes.Cut:
                result = 0.4f; // Native .1f
                break;
            case DamageTypes.Pierce:
                result = .45f; // Native .25f
                break;
        }

        return result;
    }
}
