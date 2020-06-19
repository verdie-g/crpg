using Crpg.Application.Common.Interfaces;

namespace Crpg.Infrastructure
{
    internal class ApplicationEnvironment : IApplicationEnvironment
    {
        public HostingEnvironment Environment { get; }
        public string Name { get; }

        public ApplicationEnvironment(HostingEnvironment env, string name)
        {
            Environment = env;
            Name = name;
        }
    }
}