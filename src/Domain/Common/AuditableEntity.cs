using System;

namespace Crpg.Domain.Common
{
    public class AuditableEntity
    {
        // these columns are put at the start of the table
        // https://github.com/dotnet/efcore/issues/11314
        public DateTimeOffset LastModifiedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}