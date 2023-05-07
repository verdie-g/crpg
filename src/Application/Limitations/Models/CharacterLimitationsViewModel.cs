using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Limitations;

namespace Crpg.Application.Limitations.Models;

public record CharacterLimitationsViewModel : IMapFrom<CharacterLimitations>
{
    public DateTime lastRespecializeAt { get; init; }
}
