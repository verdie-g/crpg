using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands
{
    public class UpdateCharacterStatisticsCommand : IRequest<CharacterStatisticsViewModel>
    {
        public int UserId { get; set; }
        public int CharacterId { get; set; }
        public CharacterStatisticsViewModel Statistics { get; set; } = new CharacterStatisticsViewModel();

        public class Handler : IRequestHandler<UpdateCharacterStatisticsCommand, CharacterStatisticsViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<CharacterStatisticsViewModel> Handle(UpdateCharacterStatisticsCommand req,
                CancellationToken cancellationToken)
            {
                var character = await _db.Characters.FirstOrDefaultAsync(c =>
                        c.UserId == req.UserId && c.Id == req.CharacterId, cancellationToken);
                if (character == null)
                {
                    throw new NotFoundException(nameof(Character), req.CharacterId);
                }

                SetStatistics(character.Statistics, req.Statistics);
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<CharacterStatisticsViewModel>(character.Statistics);
            }

            private void SetStatistics(CharacterStatistics stats, CharacterStatisticsViewModel newStats)
            {
                int attributesDelta = CheckedDelta(stats.Attributes.Strength, newStats.Attributes.Strength)
                    + CheckedDelta(stats.Attributes.Agility, newStats.Attributes.Agility);
                if (attributesDelta > stats.Attributes.Points)
                {
                    throw new BadRequestException("Not enough points for attributes");
                }

                int skillsDelta = CheckedDelta(stats.Skills.Athletics, newStats.Skills.Athletics)
                    + CheckedDelta(stats.Skills.HorseArchery, newStats.Skills.HorseArchery)
                    + CheckedDelta(stats.Skills.IronFlesh, newStats.Skills.IronFlesh)
                    + CheckedDelta(stats.Skills.PowerDraw, newStats.Skills.PowerDraw)
                    + CheckedDelta(stats.Skills.PowerStrike, newStats.Skills.PowerStrike)
                    + CheckedDelta(stats.Skills.PowerThrow, newStats.Skills.PowerThrow)
                    + CheckedDelta(stats.Skills.Riding, newStats.Skills.Riding)
                    + CheckedDelta(stats.Skills.Shield, newStats.Skills.Shield)
                    + CheckedDelta(stats.Skills.WeaponMaster, newStats.Skills.WeaponMaster);
                if (skillsDelta > stats.Skills.Points)
                {
                    throw new BadRequestException("Not enough points for skills");
                }

                int weaponProficienciesDelta =
                    CheckedDelta(stats.WeaponProficiencies.OneHanded, newStats.WeaponProficiencies.OneHanded)
                    + CheckedDelta(stats.WeaponProficiencies.TwoHanded, newStats.WeaponProficiencies.TwoHanded)
                    + CheckedDelta(stats.WeaponProficiencies.Polearm, newStats.WeaponProficiencies.Polearm)
                    + CheckedDelta(stats.WeaponProficiencies.Bow, newStats.WeaponProficiencies.Bow)
                    + CheckedDelta(stats.WeaponProficiencies.Throwing, newStats.WeaponProficiencies.Throwing)
                    + CheckedDelta(stats.WeaponProficiencies.Crossbow, newStats.WeaponProficiencies.Crossbow);
                if (weaponProficienciesDelta > stats.WeaponProficiencies.Points)
                {
                    throw new BadRequestException("Not enough points for weapon proficiencies");
                }

                stats.Attributes.Points -= attributesDelta;
                stats.Attributes.Agility = newStats.Attributes.Agility;
                stats.Attributes.Strength = newStats.Attributes.Strength;

                stats.Skills.Points -= skillsDelta;
                stats.Skills.Athletics = newStats.Skills.Athletics;
                stats.Skills.HorseArchery = newStats.Skills.HorseArchery;
                stats.Skills.IronFlesh = newStats.Skills.IronFlesh;
                stats.Skills.PowerDraw = newStats.Skills.PowerDraw;
                stats.Skills.PowerStrike = newStats.Skills.PowerStrike;
                stats.Skills.PowerThrow = newStats.Skills.PowerThrow;
                stats.Skills.Riding = newStats.Skills.Riding;
                stats.Skills.Shield = newStats.Skills.Shield;
                stats.Skills.WeaponMaster = newStats.Skills.WeaponMaster;

                stats.WeaponProficiencies.Points -= weaponProficienciesDelta;
                stats.WeaponProficiencies.OneHanded = newStats.WeaponProficiencies.OneHanded;
                stats.WeaponProficiencies.TwoHanded = newStats.WeaponProficiencies.TwoHanded;
                stats.WeaponProficiencies.Polearm = newStats.WeaponProficiencies.Polearm;
                stats.WeaponProficiencies.Bow = newStats.WeaponProficiencies.Bow;
                stats.WeaponProficiencies.Throwing = newStats.WeaponProficiencies.Throwing;
                stats.WeaponProficiencies.Crossbow = newStats.WeaponProficiencies.Crossbow;
            }

            private int CheckedDelta(int oldStat, int newStat)
            {
                int delta = newStat - oldStat;
                if (delta >= 0)
                {
                    return delta;
                }

                throw new BadRequestException("Can't decrease stat");
            }
        }
    }
}