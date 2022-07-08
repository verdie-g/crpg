using TaleWorlds.GauntletUI;
using TaleWorlds.TwoDimension;

namespace Crpg.Module.GUI;

// All classes inheriting Widget are usable in prefabs as an xml that (e.g. <ExperienceBarWidget />).
internal class ExperienceBarWidget : Widget
{
    private const int SpritesNb = 50;
    private const string LeftSpritePrefix = "crpg_experience_circle_left_";
    private const string RightSpritePrefix = "crpg_experience_circle_right_";

    private readonly Sprite[] _leftSprites;
    private readonly Sprite[] _rightSprites;
    private readonly int _spriteHeight;
    private float _levelProgression;

    private Widget LeftWidget => Children[1];
    private Widget RightWidget => Children[2];

    public ExperienceBarWidget(UIContext context)
        : base(context)
    {
        LoadSprites();
        (_leftSprites, _rightSprites) = LoadSprites();
        _spriteHeight = _leftSprites.Last().Height;
    }

    [Editor]
    public float LevelProgression // Wanted to use double but it is not supported (see WidgetExtensions.SetWidgetAttributeFromString).
    {
        get => _levelProgression;
        set
        {
            _levelProgression = value;

            int rightSpriteIndex;
            if (_levelProgression < 0.5f)
            {
                LeftWidget.Sprite = null;
                rightSpriteIndex = (int)Math.Round(2 * _levelProgression * (_rightSprites.Length - 1));
            }
            else
            {
                int leftSpriteIndex = (int)Math.Round(2 * (_levelProgression - 0.5f) * (_leftSprites.Length - 1));
                LeftWidget.Sprite = _leftSprites[leftSpriteIndex];
                LeftWidget.SuggestedHeight = (int)(SuggestedHeight * ((float)LeftWidget.Sprite.Height / _spriteHeight));
                LeftWidget.MarginTop = SuggestedHeight - LeftWidget.SuggestedHeight;
                rightSpriteIndex = _rightSprites.Length - 1;
            }

            RightWidget.Sprite = _rightSprites[rightSpriteIndex];
            RightWidget.SuggestedHeight = (int)(SuggestedHeight * ((float)RightWidget.Sprite.Height / _spriteHeight));
            RightWidget.MarginBottom = SuggestedHeight - RightWidget.SuggestedHeight;
        }
    }

    private (Sprite[] leftSprites, Sprite[] rightSprites) LoadSprites()
    {
        var leftSprites = Enumerable.Range(0, SpritesNb)
            .Select(n => Context.SpriteData.GetSprite(LeftSpritePrefix + n))
            .ToArray();
        var rightSprites = Enumerable.Range(0, SpritesNb)
            .Select(n => Context.SpriteData.GetSprite(RightSpritePrefix + n))
            .ToArray();
        return (leftSprites, rightSprites);
    }
}
