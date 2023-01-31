namespace Crpg.Module.Api.Models.ActivityLogs;

internal class CrpgActivityLog
{
    public int Id { get; set; }
    public CrpgActivityLogType Type { get; set; }
    public int UserId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
