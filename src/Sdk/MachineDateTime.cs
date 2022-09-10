using Crpg.Sdk.Abstractions;

namespace Crpg.Sdk;

public class MachineDateTime : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}
