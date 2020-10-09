using Crpg.Sdk.Abstractions;

namespace Crpg.Application.Common.Interfaces
{
    public interface IApplicationEnvironment
    {
        HostingEnvironment Environment { get; }
        string ServiceName { get; }
        string HostName { get; }
    }
}
