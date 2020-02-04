using System;

namespace Trpg.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string entityName, params object[] keys)
            : base($"Entity \"{entityName}\" ({string.Join(",", keys)}) was not found.")
        {
        }
    }
}