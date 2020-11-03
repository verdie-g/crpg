using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class UpdateCharacterStatisticsCommand : IMediatorRequest<CharacterStatisticsViewModel>
    {
        public int UserId { get; set; }
        public int CharacterId { get; set; }
        public CharacterStatisticsViewModel Statistics { get; set; } = new CharacterStatisticsViewModel();

        public class Handler : IMediatorRequestHandler<UpdateCharacterStatisticsCommand, CharacterStatisticsViewModel>
        {
            private static int WeaponProficienciesPointsForAgility(int agility) => agility * 14;

            private static int WeaponProficienciesPointsForWeaponMaster(int weaponMaster)
            {
                const int a = 10;
                const int b = 65;
                return weaponMaster == 0
                    ? 0
                    : a * weaponMaster * weaponMaster + b * weaponMaster;
            }

            private static int WeaponProficiencyCost(int wpf)
            {
                const float a = 0.0005f;
                const int b = 3;
                return (int)Math.Floor(a * wpf * (wpf + 1) * (2 * wpf + 1) / 6f + b * wpf);
            }

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<CharacterStatisticsViewModel>> Handle(UpdateCharacterStatisticsCommand req,
                CancellationToken cancellationToken)
            {
                var character = await _db.Characters.FirstOrDefaultAsync(c =>
                        c.UserId == req.UserId && c.Id == req.CharacterId, cancellationToken);
                if (character == null)
                {
                    return new Result<CharacterStatisticsViewModel>(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
                }

                IList<Error>? errors;
                try
                {
                    var res = SetStatistics(character.Statistics, req.Statistics);
                    errors = res.Errors;
                }
                catch (StatisticDecreasedException)
                {
                    errors = new[] { CommonErrors.StatisticDecreased() };
                }

                if (errors != null && errors.Count != 0)
                {
                    return new Result<CharacterStatisticsViewModel>(errors);
                }

                await _db.SaveChangesAsync(cancellationToken);
                return new Result<CharacterStatisticsViewModel>(_mapper.Map<CharacterStatisticsViewModel>(character.Statistics));
            }

            private Result SetStatistics(CharacterStatistics stats, CharacterStatisticsViewModel newStats)
            {
                int attributesDelta = CheckedDelta(stats.Attributes.Strength, newStats.Attributes.Strength)
                    + CheckedDelta(stats.Attributes.Agility, newStats.Attributes.Agility);
                if (attributesDelta > stats.Attributes.Points)
                {
                    return new Result(CommonErrors.NotEnoughAttributePoints(attributesDelta, stats.Attributes.Points));
                }

                stats.WeaponProficiencies.Points += WeaponProficienciesPointsForAgility(newStats.Attributes.Agility)
                    - WeaponProficienciesPointsForAgility(stats.Attributes.Agility);

                int skillsDelta = CheckedDelta(stats.Skills.IronFlesh, newStats.Skills.IronFlesh)
                    + CheckedDelta(stats.Skills.PowerStrike, newStats.Skills.PowerStrike)
                    + CheckedDelta(stats.Skills.PowerDraw, newStats.Skills.PowerDraw)
                    + CheckedDelta(stats.Skills.PowerThrow, newStats.Skills.PowerThrow)
                    + CheckedDelta(stats.Skills.Athletics, newStats.Skills.Athletics)
                    + CheckedDelta(stats.Skills.Riding, newStats.Skills.Riding)
                    + CheckedDelta(stats.Skills.WeaponMaster, newStats.Skills.WeaponMaster)
                    + CheckedDelta(stats.Skills.HorseArchery, newStats.Skills.HorseArchery)
                    + CheckedDelta(stats.Skills.Shield, newStats.Skills.Shield);
                if (skillsDelta > stats.Skills.Points)
                {
                    return new Result(CommonErrors.NotEnoughSkillPoints(skillsDelta, stats.Skills.Points));
                }

                stats.WeaponProficiencies.Points += WeaponProficienciesPointsForWeaponMaster(newStats.Skills.WeaponMaster)
                    - WeaponProficienciesPointsForWeaponMaster(stats.Skills.WeaponMaster);

                int weaponProficienciesDelta =
                    CheckedDelta(stats.WeaponProficiencies.OneHanded, newStats.WeaponProficiencies.OneHanded, WeaponProficiencyCost)
                    + CheckedDelta(stats.WeaponProficiencies.TwoHanded, newStats.WeaponProficiencies.TwoHanded, WeaponProficiencyCost)
                    + CheckedDelta(stats.WeaponProficiencies.Polearm, newStats.WeaponProficiencies.Polearm, WeaponProficiencyCost)
                    + CheckedDelta(stats.WeaponProficiencies.Bow, newStats.WeaponProficiencies.Bow, WeaponProficiencyCost)
                    + CheckedDelta(stats.WeaponProficiencies.Throwing, newStats.WeaponProficiencies.Throwing, WeaponProficiencyCost)
                    + CheckedDelta(stats.WeaponProficiencies.Crossbow, newStats.WeaponProficiencies.Crossbow, WeaponProficiencyCost);
                if (weaponProficienciesDelta > stats.WeaponProficiencies.Points)
                {
                    return new Result(CommonErrors.NotEnoughWeaponProficiencyPoints(weaponProficienciesDelta, stats.WeaponProficiencies.Points));
                }

                if (!CheckSkillsRequirement(newStats))
                {
                    return new Result(CommonErrors.SkillRequirementNotMet());
                }

                stats.Attributes.Points -= attributesDelta;
                stats.Attributes.Agility = newStats.Attributes.Agility;
                stats.Attributes.Strength = newStats.Attributes.Strength;

                stats.Skills.Points -= skillsDelta;
                stats.Skills.IronFlesh = newStats.Skills.IronFlesh;
                stats.Skills.PowerStrike = newStats.Skills.PowerStrike;
                stats.Skills.PowerDraw = newStats.Skills.PowerDraw;
                stats.Skills.PowerThrow = newStats.Skills.PowerThrow;
                stats.Skills.Athletics = newStats.Skills.Athletics;
                stats.Skills.Riding = newStats.Skills.Riding;
                stats.Skills.WeaponMaster = newStats.Skills.WeaponMaster;
                stats.Skills.HorseArchery = newStats.Skills.HorseArchery;
                stats.Skills.Shield = newStats.Skills.Shield;

                stats.WeaponProficiencies.Points -= weaponProficienciesDelta;
                stats.WeaponProficiencies.OneHanded = newStats.WeaponProficiencies.OneHanded;
                stats.WeaponProficiencies.TwoHanded = newStats.WeaponProficiencies.TwoHanded;
                stats.WeaponProficiencies.Polearm = newStats.WeaponProficiencies.Polearm;
                stats.WeaponProficiencies.Bow = newStats.WeaponProficiencies.Bow;
                stats.WeaponProficiencies.Throwing = newStats.WeaponProficiencies.Throwing;
                stats.WeaponProficiencies.Crossbow = newStats.WeaponProficiencies.Crossbow;

                return new Result();
            }

            private static int CheckedDelta(int oldStat, int newStat, Func<int, int>? cost = null)
            {
                int delta = cost == null
                    ? newStat - oldStat
                    : cost(newStat) - cost(oldStat);
                if (delta >= 0)
                {
                    return delta;
                }

                throw new StatisticDecreasedException();
            }

            private static bool CheckSkillsRequirement(CharacterStatisticsViewModel stats)
            {
                return stats.Skills.IronFlesh <= stats.Attributes.Strength / 3
                    && stats.Skills.PowerStrike <= stats.Attributes.Strength / 3
                    && stats.Skills.PowerDraw <= stats.Attributes.Strength / 3
                    && stats.Skills.PowerThrow <= stats.Attributes.Strength / 3
                    && stats.Skills.Athletics <= stats.Attributes.Agility / 3
                    && stats.Skills.Riding <= stats.Attributes.Agility / 3
                    && stats.Skills.WeaponMaster <= stats.Attributes.Agility / 3
                    && stats.Skills.HorseArchery <= stats.Attributes.Agility / 6
                    && stats.Skills.Shield <= stats.Attributes.Agility / 6;
            }

            private class StatisticDecreasedException : Exception
            {
            }
        }
    }
}
