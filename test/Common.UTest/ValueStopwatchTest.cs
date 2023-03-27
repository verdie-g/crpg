using NUnit.Framework;

namespace Crpg.Common.UTest;

public class ValueStopwatchTest
{
    [Test]
    public async Task TestElapsed()
    {
        var sw = ValueStopwatch.StartNew();

        await Task.Delay(200);
        Assert.That(sw.Elapsed.TotalMilliseconds, Is.GreaterThan(150));
        Assert.That(sw.Elapsed.TotalMilliseconds, Is.LessThan(500));

        await Task.Delay(200);
        Assert.That(sw.Elapsed.TotalMilliseconds, Is.GreaterThan(350));
        Assert.That(sw.Elapsed.TotalMilliseconds, Is.LessThan(1000));
    }
}
