namespace Crpg.Domain.Common;

public class AuditableEntity
{
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
