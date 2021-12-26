using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Settlements;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Settlements.Models;

public record SettlementCreation
{
    public string Name { get; init; } = string.Empty;
    public SettlementType Type { get; init; }
    public Culture Culture { get; init; }
    public Point Position { get; init; } = default!;
    public string Scene { get; init; } = string.Empty;
}
