using System;

namespace Crpg.Application.Common.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string entityName, object key)
            : base($"Not authorized to access \"{entityName}\" ({key})")
        {
        }
    }
}