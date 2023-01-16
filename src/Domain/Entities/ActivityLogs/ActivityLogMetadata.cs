namespace Crpg.Domain.Entities.ActivityLogs;

public class ActivityLogMetadata
{
    public ActivityLogMetadata(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public int ActivityLogId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}
