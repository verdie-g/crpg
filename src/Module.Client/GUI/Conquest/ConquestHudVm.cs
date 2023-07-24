using Crpg.Module.GUI.Hud;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Conquest;

internal class ConquestHudVm : ViewModel
{
    private FlagsHudVm _flagsVm;
    private BannerHudVm _allyBannerVm;
    private BannerHudVm _enemyBannerVm;
    private TimerHudVm _timerVm;

    public ConquestHudVm(Mission mission)
    {
        _flagsVm = new FlagsHudVm();
        _allyBannerVm = new BannerHudVm(mission, allyBanner: true);
        _enemyBannerVm = new BannerHudVm(mission, allyBanner: false);
        _timerVm = new TimerHudVm(mission);
    }

    [DataSourceProperty]
    public FlagsHudVm Flags
    {
        get => _flagsVm;
        set
        {
            if (value != _flagsVm)
            {
                _flagsVm = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public BannerHudVm AllyBanner
    {
        get => _allyBannerVm;
        set
        {
            if (value != _allyBannerVm)
            {
                _allyBannerVm = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public BannerHudVm EnemyBanner
    {
        get => _enemyBannerVm;
        set
        {
            if (value != _enemyBannerVm)
            {
                _enemyBannerVm = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public TimerHudVm Timer
    {
        get => _timerVm;
        set
        {
            if (value != _timerVm)
            {
                _timerVm = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    public void Tick(float dt)
    {
        _flagsVm.Tick(dt);
        _timerVm.Tick(dt);
    }
}
