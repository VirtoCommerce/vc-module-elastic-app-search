using System.Text.RegularExpressions;

namespace VirtoCommerce.ElasticAppSearch.Tests.Extensions;

public static class StringExtensions
{
    private static readonly Regex WhitespaceRegex = new(@"\s");

    public static string ReplaceWhitespaces(this string str, string replacement)
    {
        return WhitespaceRegex.Replace(str, replacement);
    }
}
