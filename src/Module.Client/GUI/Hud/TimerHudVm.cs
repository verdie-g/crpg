using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Hud;

public class TimerHudVm : ViewModel
{
    private const float RemainingTimeWarningThreshold = 5f;

    private readonly MissionMultiplayerGameModeBaseClient _gameMode;
    private bool _warnRemainingTime;
    private string? _remainingRoundTime;
    private int _generalWarningCountdown;
    private bool _isGeneralWarningCountdownActive;

    public TimerHudVm(Mission mission)
    {
        _gameMode = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        if (_gameMode.RoundComponent != null)
        {
            _gameMode.RoundComponent.OnCurrentRoundStateChanged += OnCurrentGameModeStateChanged;
        }

        if (_gameMode.WarmupComponent != null)
        {
            _gameMode.WarmupComponent.OnWarmupEnded += OnCurrentGameModeStateChanged;
        }
    }

    [DataSourceProperty]
    public string? RemainingRoundTime
    {
        get
        {
            return _remainingRoundTime;
        }
        set
        {
            if (value == _remainingRoundTime)
            {
                return;
            }

            _remainingRoundTime = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool WarnRemainingTime
    {
        get
        {
            return _warnRemainingTime;
        }
        set
        {
            if (value == _warnRemainingTime)
            {
                return;
            }

            _warnRemainingTime = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public int GeneralWarningCountdown
    {
        get
        {
            return _generalWarningCountdown;
        }
        set
        {
            if (value == _generalWarningCountdown)
            {
                return;
            }

            _generalWarningCountdown = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool IsGeneralWarningCountdownActive
    {
        get
        {
            return _isGeneralWarningCountdownActive;
        }
        set
        {
            if (value == _isGeneralWarningCountdownActive)
            {
                return;
            }

            _isGeneralWarningCountdownActive = value;
            OnPropertyChangedWithValue(value);
        }
    }

    public void Tick(float dt)
    {
        CheckTimers();
    }

    public override void OnFinalize()
    {
        if (_gameMode.RoundComponent != null)
        {
            _gameMode.RoundComponent.OnCurrentRoundStateChanged -= OnCurrentGameModeStateChanged;
        }

        if (_gameMode.WarmupComponent != null)
        {
            _gameMode.WarmupComponent.OnWarmupEnded -= OnCurrentGameModeStateChanged;
        }
    }

    private void CheckTimers(bool forceUpdate = false)
    {
        if (!_gameMode.CheckTimer(out int remainingTime, out int remainingWarningTime, forceUpdate))
        {
            return;
        }

        RemainingRoundTime = TimeSpan.FromSeconds(remainingTime).ToString("mm':'ss");
        WarnRemainingTime = remainingTime <= RemainingTimeWarningThreshold;
        if (GeneralWarningCountdown == remainingWarningTime)
        {
            return;
        }

        IsGeneralWarningCountdownActive = remainingWarningTime > 0;
        GeneralWarningCountdown = remainingWarningTime;
    }

    private void OnCurrentGameModeStateChanged()
    {
        CheckTimers(true);
    }
}
