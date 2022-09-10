using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI;

internal class CrpgAgentHudViewModel : ViewModel
{
    private readonly CrpgExperienceTable _experienceTable;
    private readonly NetworkCommunicator _myPeer;
    private int _experience;
    private float _levelProgression;
    private int _rewardMultiplier;
    private string _rewardMultiplierStr = string.Empty;
    private bool _showExperienceBar;

    public CrpgAgentHudViewModel(CrpgExperienceTable experienceTable)
    {
        _experienceTable = experienceTable;
        _myPeer = GameNetwork.MyPeer;
    }

    /// <summary>
    /// A number between 0.0 and 1.0 for the level progression of the player.
    /// </summary>
    [DataSourceProperty]
    public float LevelProgression
    {
        get => _levelProgression;
        private set
        {
            _levelProgression = value;
            OnPropertyChangedWithValue(value); // Notify that the property changed.
        }
    }

    [DataSourceProperty]
    public string RewardMultiplier
    {
        get => _rewardMultiplierStr;
        private set
        {
            _rewardMultiplierStr = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool ShowExperienceBar
    {
        get => _showExperienceBar;
        private set
        {
            _showExperienceBar = value;
            OnPropertyChangedWithValue(value);
        }
    }

    public void Tick(float deltaTime)
    {
        // Hide the experience bar if the user is dead.
        ShowExperienceBar = Mission.Current?.MainAgent != null;

        var crpgRepresentative = _myPeer.GetComponent<CrpgRepresentative>();
        if (crpgRepresentative == null)
        {
            return;
        }

        var user = crpgRepresentative.User;
        if (user == null)
        {
            return;
        }

        if (_experience != user.Character.Experience)
        {
            float levelProgression = ComputeLevelProgression(user);
            // Clamp if for some reason the level and experience are not synchronized.
            LevelProgression = MathF.Clamp(levelProgression, 0.0f, 1.0f);
            _experience = user.Character.Experience;
        }

        if (_rewardMultiplier != crpgRepresentative.RewardMultiplier)
        {
            RewardMultiplier = 'x' + crpgRepresentative.RewardMultiplier.ToString();
            _rewardMultiplier = crpgRepresentative.RewardMultiplier;
        }
    }

    private float ComputeLevelProgression(CrpgUser user)
    {
        int experienceForCurrentLevel = _experienceTable.GetExperienceForLevel(user.Character.Level);
        int experienceForNextLevel = _experienceTable.GetExperienceForLevel(user.Character.Level + 1);
        return (float)(user.Character.Experience - experienceForCurrentLevel) / (experienceForNextLevel - experienceForCurrentLevel);
    }
}
