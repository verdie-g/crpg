using System;

namespace Trpg.Persistence.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(Exception innerException)
            : base("Conflict detected", innerException)
        {
        }
    }
}