namespace Crpg.Application.UTest;

internal static class TestHelper
{
    public static string FindClosestString(string a, IEnumerable<string> allStrings)
    {
        string closest = string.Empty;
        int closestDistance = int.MaxValue;

        foreach (string b in allStrings)
        {
            int d = ComputeStringDistance(a, b);
            if (d < closestDistance)
            {
                closest = b;
                closestDistance = d;
            }
        }

        return closest;
    }

    private static int ComputeStringDistance(string a, string b)
    {
        if (a.Length == 0)
        {
            return b.Length;
        }

        if (b.Length == 0)
        {
            return a.Length;
        }

        int[,] d = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i < a.Length + 1; i += 1)
        {
            d[i, 0] = i;
        }

        for (int j = 0; j < b.Length + 1; j += 1)
        {
            d[0, j] = j;
        }

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = (b[j - 1] != a[i - 1]) ? 1 : 0;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[a.Length, b.Length];
    }
}
