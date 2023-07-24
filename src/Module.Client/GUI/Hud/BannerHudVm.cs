using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Hud;

internal class BannerHudVm : ViewModel
{
    private readonly Mission _mission;
    private readonly bool _allyBanner;
    private ImageIdentifierVM? _banner;

    public BannerHudVm(Mission mission, bool allyBanner)
    {
        _mission = mission;
        _allyBanner = allyBanner;
        MissionPeer.OnTeamChanged += OnTeamChanged;
    }

    [DataSourceProperty]
    public ImageIdentifierVM? Banner
    {
        get => _banner;
        set
        {
            if (value != _banner)
            {
                _banner = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    public override void OnFinalize()
    {
        MissionPeer.OnTeamChanged -= OnTeamChanged;
    }

    private void OnTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
    {
        var enemyBanner = newTeam == _mission.AttackerTeam ? _mission.DefenderTeam : _mission.AttackerTeam;
        var bannerCode = _allyBanner
            ? newTeam.Banner
            : enemyBanner?.Banner; // For some reason mission teams might not be initialized here.
        Banner = new ImageIdentifierVM(BannerCode.CreateFrom(bannerCode), true);
    }
}
