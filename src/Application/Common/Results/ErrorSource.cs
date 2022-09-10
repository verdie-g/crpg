namespace Crpg.Application.Common.Results;

/// <summary>
/// Structure containing references to the source of an error.
/// </summary>
public class ErrorSource
{
    /// <summary>
    /// Pointer is a JSON Pointer [RFC6901] to the associated entity in the request document.
    /// </summary>
    /// <example>
    /// "/data" for a primary data object, or "/data/attributes/title" for a specific attribute.
    /// </example>
    public string? Pointer { get; init; }

    /// <summary>
    /// Parameter is a string indicating which URI query parameter caused the error.
    /// </summary>
    public string? Parameter { get; init; }
}
