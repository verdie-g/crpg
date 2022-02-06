namespace Crpg.GameMod.Api.Models;

internal class CrpgResult<TData> where TData : class
{
    public TData? Data { get; set; }
}
