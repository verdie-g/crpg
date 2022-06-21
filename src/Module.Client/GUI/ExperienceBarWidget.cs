using TaleWorlds.GauntletUI;
using TaleWorlds.TwoDimension;

namespace Crpg.Module.GUI;

// All classes inheriting Widget are usable in prefabs as an xml that (e.g. <ExperienceBarWidget />).
internal class ExperienceBarWidget : Widget
{
    private const int SpritesNb = 16;
    private const string SpritePrefix = "crpg_experience_circle_";

    private readonly Sprite[] _sprites;
    private float _levelProgression;

    public ExperienceBarWidget(UIContext context)
        : base(context)
    {
        _sprites = LoadSprites();
    }

    [Editor]
    public float LevelProgression // Wanted to use double but it is not supported (see WidgetExtensions.SetWidgetAttributeFromString).
    {
        get => _levelProgression;
        set
        {
            _levelProgression = value;
            int spritesIndex = (int)Math.Round(_levelProgression * (_sprites.Length - 1));
            Sprite = _sprites[spritesIndex];
        }
    }

    private Sprite[] LoadSprites()
    {
        return Enumerable.Range(0, SpritesNb)
            .Select(n => Context.SpriteData.GetSprite(SpritePrefix + n))
            .ToArray();
    }
}
