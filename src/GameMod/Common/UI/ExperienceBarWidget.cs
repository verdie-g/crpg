using System;
using System.Linq;
using System.Threading;
using TaleWorlds.GauntletUI;
using TaleWorlds.TwoDimension;

namespace Crpg.GameMod.Common.UI
{
    internal class ExperienceBarWidget : Widget
    {
        private const int SpritesNb = 16;
        private const string SpritePrefix = "crpg_experience_circle_";

        private readonly Sprite[] _sprites;
        private int _i = 0;
        private Timer _timer;

        public ExperienceBarWidget(UIContext context) : base(context)
        {
            _sprites = LoadSprites();
            Sprite = _sprites[_i++];
            _timer = new Timer(_ =>
            {
                _i = (_i + 1) % SpritesNb;
                Sprite = _sprites[_i];
            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        private Sprite[] LoadSprites()
        {
            return Enumerable.Range(0, SpritesNb)
                .Select(n => Context.SpriteData.GetSprite(SpritePrefix + n))
                .ToArray();
        }
    }
}
