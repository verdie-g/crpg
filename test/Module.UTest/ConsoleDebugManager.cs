using TaleWorlds.Library;

namespace Crpg.Module.UTest;

internal class ConsoleDebugManager : IDebugManager
{
    public void ShowWarning(string message)
    {
        Console.WriteLine(message);
    }

    public void Assert(bool condition, string message, string callerFile = "", string callerMethod = "", int callerLine = 0)
    {
        NUnit.Framework.Assert.IsTrue(condition, message);
    }

    public void SilentAssert(bool condition, string message = "", bool getDump = false, string callerFile = "",
        string callerMethod = "", int callerLine = 0)
    {
        NUnit.Framework.Assert.IsTrue(condition, message);
    }

    public void Print(string message, int logLevel = 0, Debug.DebugColor color = Debug.DebugColor.White,
        ulong debugFilter = 17592186044416)
    {
        Console.WriteLine(message);
    }

    public void PrintError(string error, string stackTrace, ulong debugFilter = 17592186044416)
    {
        Console.Error.WriteLine(error);
    }

    public void PrintWarning(string warning, ulong debugFilter = 17592186044416)
    {
        Console.WriteLine(warning);
    }

    public void ShowError(string message)
    {
        Console.Error.WriteLine(message);
    }

    public void ShowMessageBox(string lpText, string lpCaption, uint uType)
    {
        Console.Error.WriteLine(lpText);
    }

    public void DisplayDebugMessage(string message)
    {
        Console.Error.WriteLine(message);
    }

    public void WatchVariable(string name, object value)
    {
    }

    public void BeginTelemetryScopeInternal(TelemetryLevelMask levelMask, string scopeName)
    {
    }

    public void BeginTelemetryScopeBaseLevelInternal(TelemetryLevelMask levelMask, string scopeName)
    {
    }

    public void EndTelemetryScopeInternal()
    {
    }

    public void EndTelemetryScopeBaseLevelInternal()
    {
    }

    public void WriteDebugLineOnScreen(string message)
    {
        Console.Error.WriteLine(message);
    }

    public void RenderDebugLine(Vec3 position, Vec3 direction, uint color = 4294967295, bool depthCheck = false, float time = 0)
    {
    }

    public void RenderDebugSphere(Vec3 position, float radius, uint color = 4294967295, bool depthCheck = false, float time = 0)
    {
    }

    public void RenderDebugText3D(Vec3 position, string text, uint color = 4294967295, int screenPosOffsetX = 0,
        int screenPosOffsetY = 0, float time = 0)
    {
    }

    public void RenderDebugFrame(MatrixFrame frame, float lineLength, float time = 0)
    {
    }

    public void RenderDebugText(float screenX, float screenY, string text, uint color = 4294967295, float time = 0)
    {
    }

    public void RenderDebugRectWithColor(float left, float bottom, float right, float top, uint color = 4294967295)
    {
    }

    public Vec3 GetDebugVector()
    {
        return Vec3.Zero;
    }

    public void SetCrashReportCustomString(string customString)
    {
    }

    public void SetCrashReportCustomStack(string customStack)
    {
    }

    public void SetTestModeEnabled(bool testModeEnabled)
    {
    }

    public void AbortGame()
    {
    }

    public void DoDelayedexit(int returnCode)
    {
    }

    public void ReportMemoryBookmark(string message)
    {
    }
}
