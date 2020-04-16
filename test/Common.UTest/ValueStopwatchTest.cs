using System.Threading.Tasks;
using NUnit.Framework;

namespace Crpg.Common.UTest
{
    public class ValueStopwatchTest
    {
        [Test]
        public async Task TestElapsed()
        {
            var sw = ValueStopwatch.StartNew();

            await Task.Delay(200);
            Assert.Greater(sw.Elapsed.TotalMilliseconds, 150);
            Assert.Less(sw.Elapsed.TotalMilliseconds, 250);

            await Task.Delay(200);
            Assert.Greater(sw.Elapsed.TotalMilliseconds, 350);
            Assert.Less(sw.Elapsed.TotalMilliseconds, 500);
        }
    }
}