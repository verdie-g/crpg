using Crpg.Domain.Entities.ActivityLogs;

namespace Crpg.Application.ActivityLogs.Models;

public record ActivityLogViewModel
{
    public int Id { get; init; }
    public ActivityLogType Type { get; init; }
    public int UserId { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}
