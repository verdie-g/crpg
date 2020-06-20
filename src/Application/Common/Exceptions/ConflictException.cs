using System;

namespace Crpg.Application.Common.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException()
            : this(null)
        {
        }

        public ConflictException(Exception? innerException)
            : base("Conflict detected", innerException)
        {
        }
    }
}