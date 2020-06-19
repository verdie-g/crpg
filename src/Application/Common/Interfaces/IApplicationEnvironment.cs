namespace Crpg.Application.Common.Interfaces
{
    public interface IApplicationEnvironment
    {
        HostingEnvironment Environment { get; }
        string Name { get; }
    }
}