﻿using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.DefendTheVirgin;

internal class CrpgLogic : MissionLogic
{
    private readonly WaveController _waveController;
    private readonly ICrpgClient _crpgClient;
    private readonly WaveGroup[][] _waves;
    private readonly CrpgUserAccessor _userAccessor;

    public CrpgLogic(WaveController waveController, ICrpgClient crpgClient, WaveGroup[][] waves, CrpgUserAccessor userAccessor)
    {
        _waveController = waveController;
        _crpgClient = crpgClient;
        _waves = waves;
        _userAccessor = userAccessor;
        _waveController.OnWaveEnding += OnWaveEnding;
    }

    protected override void OnEndMission()
    {
        _waveController.OnWaveEnding -= OnWaveEnding;
    }

    private void OnWaveEnding(int waveNb, Team winnerTeam)
    {
        // If bots won, don't give reward.
        if (winnerTeam == Mission.AttackerTeam)
        {
            return;
        }

        WaveGroup[] wave = _waves[waveNb - 1];
        int experienceReward = ComputeWaveTier(wave) * 100;
        int goldReward = experienceReward / 20;
        InformationManager.DisplayMessage(new InformationMessage($"Gained {experienceReward} experience.", new Color(218, 112, 214)));
        InformationManager.DisplayMessage(new InformationMessage($"Gained {goldReward} gold.", new Color(65, 105, 225)));

        Task.Run(() => SendReward(experienceReward, goldReward));
    }

    private async Task SendReward(int experienceReward, int goldReward)
    {
        var res = await _crpgClient.UpdateUsersAsync(new CrpgGameUsersUpdateRequest
        {
            Updates = new[]
            {
                new CrpgUserUpdate
                {
                    CharacterId = _userAccessor.User.Character.Id,
                    Reward = new CrpgUserReward
                    {
                        Experience = experienceReward,
                        Gold = goldReward,
                    },
                },
            },
        });

        if (res.Data!.UpdateResults[0].User.Character.Level != _userAccessor.User.Character.Level)
        {
            InformationManager.DisplayMessage(new InformationMessage
            {
                Information = "Level up!",
                Color = new Color(128, 0, 128),
                SoundEventPath = "event:/ui/notification/levelup",
            });
        }

        _userAccessor.User = res.Data!.UpdateResults[0].User;
    }

    private int ComputeWaveTier(IEnumerable<WaveGroup> wave)
    {
        float value = 0;
        foreach (var group in wave)
        {
            BasicCharacterObject character = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(group.Id);
            float weight = character.Equipment.GetTotalWeightOfArmor(true) + character.Equipment.GetTotalWeightOfWeapons();
            value += weight * (character.Level + 5) * group.Count;
        }

        return (int)value;
    }
}
