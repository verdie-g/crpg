using Crpg.Sdk.Abstractions;

namespace Crpg.Sdk;

internal class ApplicationEnvironment : IApplicationEnvironment
{
    public HostingEnvironment Environment { get; }
    public string ServiceName { get; }
    public string HostName { get; }
    public string Instance { get; }

    public ApplicationEnvironment(HostingEnvironment env, string serviceName, string instance, string hostName)
    {
        Environment = env;
        ServiceName = serviceName;
        Instance = instance;
        HostName = hostName;
    }
}
