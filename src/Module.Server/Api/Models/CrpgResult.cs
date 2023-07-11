namespace Crpg.Module.Api.Models;

internal class CrpgResult
{
    public IList<Error>? Errors { get; set; }
}

internal class CrpgResult<TData> : CrpgResult where TData : class
{
    public TData? Data { get; set; }
}

internal class Error
{
    public string? TraceId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public string? Source { get; set; }
    public string? StackTrace { get; set; }

    public override string ToString() => Detail ?? Title ?? Code.ToString();
}
