namespace Crpg.Domain.Entities.ActivityLogs;

public class ActivityLogMetadata
{
    public int ActivityLogId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
