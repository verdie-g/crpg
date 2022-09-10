using System.Text.RegularExpressions;

namespace Crpg.Common.Helpers;

public static class StringHelper
{
    // https://stackoverflow.com/a/37301354/5407910
    public static string PascalToKebabCase(string value)
    {
        return Regex.Replace(
                value,
                "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                "-$1",
                RegexOptions.Compiled)
            .Trim()
            .ToLower();
    }
}
