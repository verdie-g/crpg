using Crpg.Sdk.Abstractions;

namespace Crpg.Sdk
{
    internal class ApplicationEnvironment : IApplicationEnvironment
    {
        public HostingEnvironment Environment { get; }
        public string ServiceName { get; }
        public string HostName { get; }

        public ApplicationEnvironment(HostingEnvironment env, string serviceName, string hostName)
        {
            Environment = env;
            ServiceName = serviceName;
            HostName = hostName;
        }
    }
}
