using System.Collections.Generic;
using System.Threading.Tasks;
using Crpg.GameMod.Api;
using Crpg.GameMod.Api.Models;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod.DefendTheVirgin
{
    internal class CrpgLogic : MissionLogic
    {
        private readonly WaveController _waveController;
        private readonly ICrpgClient _crpgClient;
        private readonly IList<Wave> _waves;

        private CrpgUser _user;

        public CrpgLogic(WaveController waveController, ICrpgClient crpgClient, IList<Wave> waves, CrpgUser user)
        {
            _waveController = waveController;
            _crpgClient = crpgClient;
            _waves = waves;
            _user = user;
            _waveController.OnWaveEnding += OnWaveEnding;
        }

        protected override void OnEndMission()
        {
            _waveController.OnWaveEnding -= OnWaveEnding;
        }

        private void OnWaveEnding(int waveNb)
        {
            Wave wave = _waves[waveNb - 1];
            int reward = SumWaveWeight(wave);
            int experienceReward = reward * 100;
            int goldReward = reward * 5;
            InformationManager.DisplayMessage(new InformationMessage($"Gained {experienceReward} experience.", new Color(218, 112, 214)));
            InformationManager.DisplayMessage(new InformationMessage($"Gained {goldReward} gold.", new Color(65, 105, 225)));

            Task.Run(() => SendReward(experienceReward, goldReward));
        }

        private async Task SendReward(int experienceReward, int goldReward)
        {
            var res = await _crpgClient.Update(new CrpgGameUpdateRequest
            {
                GameUserUpdates = new[]
                {
                    new CrpgGameUserUpdate
                    {
                        PlatformId = _user.PlatformId,
                        CharacterName = _user.Character.Name,
                        Reward = new CrpgUserReward
                        {
                            Experience = experienceReward,
                            Gold = goldReward,
                        },
                    },
                },
            });

            if (res.Users[0].Character.Level != _user.Character.Level)
            {
                InformationManager.DisplayMessage(new InformationMessage("Level up!"));
            }

            _user = res.Users[0];
        }

        private static int SumWaveWeight(Wave wave)
        {
            float value = 0;
            foreach (var group in wave.Groups)
            {
                BasicCharacterObject character = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(group.CharacterId);
                float weight = character.Equipment.GetTotalWeightOfArmor(true) + character.Equipment.GetTotalWeightOfWeapons();
                value += weight * group.Count;
            }

            return (int)value;
        }
    }
}