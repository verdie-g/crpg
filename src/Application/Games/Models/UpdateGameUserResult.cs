namespace Crpg.Application.Games.Models;

public record UpdateGameUserResult
{
    public GameUserViewModel User { get; init; } = default!;
    public GameUserEffectiveReward EffectiveReward { get; init; } = default!;
    public IList<GameRepairedItem> RepairedItems { get; init; } = Array.Empty<GameRepairedItem>();
}
