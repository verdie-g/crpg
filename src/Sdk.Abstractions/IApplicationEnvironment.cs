namespace Crpg.Sdk.Abstractions
{
    /// <summary>
    /// Provides information about the environment an application is running in.
    /// </summary>
    public interface IApplicationEnvironment
    {
        /// <summary>
        /// Hosting environment the application is running in.
        /// </summary>
        HostingEnvironment Environment { get; }

        /// <summary>
        /// Name of the application.
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Name of the machine the application is running on.
        /// </summary>
        string HostName { get; }
    }
}
