using Crpg.Sdk.Abstractions;

namespace Crpg.Sdk;

public static class ApplicationEnvironmentProvider
{
    public static IApplicationEnvironment FromEnvironment()
    {
        string? envStr = Environment.GetEnvironmentVariable("CRPG_ENV");
        if (envStr == null || !Enum.TryParse(envStr, true, out HostingEnvironment env))
        {
            env = HostingEnvironment.Development;
        }

        string serviceName = Environment.GetEnvironmentVariable("CRPG_SERVICE") ?? "test";
        string instance = Environment.GetEnvironmentVariable("CRPG_INSTANCE") ?? string.Empty;
        string hostName = Environment.MachineName;
        return new ApplicationEnvironment(env, serviceName, instance, hostName);
    }
}
