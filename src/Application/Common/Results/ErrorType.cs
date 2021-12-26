namespace Crpg.Application.Common.Results;

/// <summary>
/// A machine-readable code specifying error category.
/// </summary>
public enum ErrorType
{
    InternalError,
    Forbidden,
    Conflict,
    NotFound,
    Validation,
}
