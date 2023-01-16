using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.ActivityLogs;

public class ActivityLog : AuditableEntity
{
    public int Id { get; set; }
    public ActivityLogType Type { get; set; }
    public int UserId { get; set; }
    public List<ActivityLogMetadata> Metadata { get; set; } = new();
}
