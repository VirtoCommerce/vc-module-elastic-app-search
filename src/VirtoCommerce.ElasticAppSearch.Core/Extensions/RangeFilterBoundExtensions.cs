using System;
using System.Globalization;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class RangeFilterBoundExtensions
{
    private delegate bool TryParseDelegate<T>(string value, out T result);
    
    public static bool TryParseFrom(bool include, string value, out RangeFilterBound<decimal> bound)
    {
        return TryParse(include, value, TryParseDecimal, result => include || result > decimal.MinValue, out bound);
    }

    public static bool TryParseFrom(bool include, string value, out RangeFilterBound<double> bound)
    {
        return TryParse(include, value, TryParseDouble,
            result => !double.IsNaN(result) && double.IsFinite(result) && (include || result > double.MinValue), out bound);
    }

    public static bool TryParseFrom(bool include, string value, out RangeFilterBound<DateTime> bound)
    {
        return TryParse(include, value, TryParseDateTime, _ => true, out bound);
    }

    public static bool TryParseTo(bool include, string value, out RangeFilterBound<decimal> bound)
    {
        return TryParse(include, value, TryParseDecimal, result => !include || result < decimal.MaxValue, out bound);
    }

    public static bool TryParseTo(bool include, string value, out RangeFilterBound<double> bound)
    {
        return TryParse(include, value, TryParseDouble,
            result => !double.IsNaN(result) && double.IsFinite(result) && (!include || result < double.MaxValue), out bound);
    }

    public static bool TryParseTo(bool include, string value, out RangeFilterBound<DateTime> bound)
    {
        return TryParse(include, value, TryParseDateTime, _ => true, out bound);
    }

    private static bool TryParse<T>(bool include, string value, TryParseDelegate<T> tryParse, Func<T, bool> isValid, out RangeFilterBound<T> bound)
    {
        bound = null;
        var successfullyParsed = tryParse(value, out var result);
        if (successfullyParsed && isValid(result))
        {
            bound = new RangeFilterBound<T>
            {
                Include = include,
                Value = result
            };
            return true;
        }

        return false;
    }

    private static bool TryParseDouble(string value, out double result)
    {
        return double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        return decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseDateTime(string value, out DateTime result)
    {
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result);
    }
}
