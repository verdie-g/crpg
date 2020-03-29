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
            Assert.Greater(sw.Elapsed.TotalMilliseconds, 190);
            Assert.Less(sw.Elapsed.TotalMilliseconds, 225);

            await Task.Delay(200);
            Assert.Greater(sw.Elapsed.TotalMilliseconds, 380);
            Assert.Less(sw.Elapsed.TotalMilliseconds, 450);
        }
    }
}