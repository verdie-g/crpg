using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crpg.Infrastructure.Files;
using NUnit.Framework;

namespace Crpg.Infrastructure.UTest.Files
{
    public class FileItemsSourceTest
    {
        [Test]
        public async Task TestCanDeserializeFile()
        {
            var source = new FileItemsSource();
            var items = await source.LoadItems();
        }

        [Test]
        public async Task CheckNoDuplicatedMbId()
        {
            var duplicates = new List<string>();
            var mbIds = new HashSet<string>();

            var items = await new FileItemsSource().LoadItems();
            foreach (var item in items)
            {
                if (mbIds.Contains(item.MbId))
                {
                    duplicates.Add(item.MbId);
                }

                mbIds.Add(item.MbId);
            }

            if (duplicates.Count != 0)
            {
                Assert.Fail("Duplicate items: " + string.Join(", ", duplicates));
            }
        }
    }
}