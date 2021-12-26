using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models;

public record CharacterSkillsViewModel : IMapFrom<CharacterSkills>
{
    public int Points { get; init; }
    public int IronFlesh { get; init; }
    public int PowerStrike { get; init; }
    public int PowerDraw { get; init; }
    public int PowerThrow { get; init; }
    public int Athletics { get; init; }
    public int Riding { get; init; }
    public int WeaponMaster { get; init; }
    public int MountedArchery { get; init; }
    public int Shield { get; init; }
}
