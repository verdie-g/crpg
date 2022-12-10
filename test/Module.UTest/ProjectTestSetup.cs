using NUnit.Framework;
using TaleWorlds.Library;

namespace Crpg.Module.UTest;

/// <summary>
/// Executed before any tests in the project.
/// </summary>
[SetUpFixture]
public class ProjectTestSetup
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Debug.DebugManager = new ConsoleDebugManager();
    }
}
