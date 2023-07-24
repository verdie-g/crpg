using Crpg.Module.GUI.Hud;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Dtv;

internal class DtvHudVm : ViewModel
{
    private BannerHudVm _allyBannerVm;
    private BannerHudVm _enemyBannerVm;
    private TimerHudVm _timerVm;

    public DtvHudVm(Mission mission)
    {
        _allyBannerVm = new BannerHudVm(mission, allyBanner: true);
        _enemyBannerVm = new BannerHudVm(mission, allyBanner: false);
        _timerVm = new TimerHudVm(mission);
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
        _timerVm.Tick(dt);
    }
}
