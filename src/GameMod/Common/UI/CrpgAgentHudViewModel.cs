using Crpg.GameMod.Api.Models.Users;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod.Common.UI
{
    internal class CrpgAgentHudViewModel : ViewModel
    {
        private readonly CrpgUserAccessor _userAccessor;
        private readonly CrpgExperienceTable _experienceTable;
        private float _levelProgression;
        private bool _showExperienceBar;

        public CrpgAgentHudViewModel(CrpgUserAccessor userAccessor, CrpgExperienceTable experienceTable)
        {
            _userAccessor = userAccessor;
            _experienceTable = experienceTable;

            _userAccessor.OnUserUpdate += UpdateLevelProgression;
            UpdateLevelProgression(userAccessor.User);
        }

        public override void OnFinalize()
        {
            _userAccessor.OnUserUpdate -= UpdateLevelProgression;
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
                OnPropertyChangedWithValue(value, nameof(LevelProgression)); // Notify that the property changed.
            }
        }

        [DataSourceProperty]
        public bool ShowExperienceBar
        {
            get => _showExperienceBar;
            private set
            {
                _showExperienceBar = value;
                OnPropertyChangedWithValue(value, nameof(ShowExperienceBar));
            }
        }

        public void Tick(float deltaTime)
        {
            // Hide the experience bar if the user is dead.
            ShowExperienceBar = Mission.Current?.MainAgent != null;
        }

        private void UpdateLevelProgression(CrpgUser user)
        {
            int experienceForCurrentLevel = _experienceTable.GetExperienceForLevel(user.Character.Level);
            int experienceForNextLevel = _experienceTable.GetExperienceForLevel(user.Character.Level + 1);
            LevelProgression = (float)(user.Character.Experience - experienceForCurrentLevel) / (experienceForNextLevel - experienceForCurrentLevel);
        }
    }
}
