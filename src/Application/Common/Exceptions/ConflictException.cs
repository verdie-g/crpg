namespace Crpg.Application.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(Exception innerException)
        : base(null, innerException)
    {
    }
}
