using TaleWorlds.Core;

namespace Crpg.Module.Common;

internal class CrpgBannerEffects
{
    private static BannerEffect _none = default!;

    public static void Initialize(Game game)
    {
        _none = new BannerEffect("None");
        _none.Initialize("{=}Does nothing", "{=}No.", 0, 0, 0, BannerEffect.EffectIncrementType.AddFactor);
        game.ObjectManager.RegisterPresumedObject(_none);
    }
}
