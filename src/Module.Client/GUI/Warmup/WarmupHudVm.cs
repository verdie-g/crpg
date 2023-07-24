using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Warmup;

internal class WarmupHudVm : ViewModel
{
    private readonly MissionMultiplayerGameModeBaseClient _gameMode;

    private string? _warmupInfoText;
    private bool _isInWarmup;

    public WarmupHudVm(Mission mission)
    {
        _gameMode = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        RefreshValues();
    }

    [DataSourceProperty]
    public bool IsInWarmup
    {
        get
        {
            return _isInWarmup;
        }
        set
        {
            if (value != _isInWarmup)
            {
                _isInWarmup = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string? WarmupInfoText
    {
        get
        {
            return _warmupInfoText;
        }
        set
        {
            if (value != _warmupInfoText)
            {
                _warmupInfoText = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    public sealed override void RefreshValues()
    {
        base.RefreshValues();

        string gameType = MultiplayerOptions.OptionType.GameType.GetStrValue();
        TextObject textObject = new("{=XJTX8w8M}Warmup Phase - {GAME_MODE}\nWaiting for players to join");
        textObject.SetTextVariable("GAME_MODE", GameTexts.FindText("str_multiplayer_official_game_type_name", gameType));
        WarmupInfoText = textObject.ToString();
    }

    public void Tick(float dt)
    {
        IsInWarmup = _gameMode.IsInWarmup;
    }
}
