using System;
using System.Globalization;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class RangeFilterBoundExtensions
{
    private delegate bool TryParseDelegate<T>(string value, out T result);
    
    public static bool TryParseFrom(bool include, string value, out RangeFilterBound<double> bound)
    {
        return TryParse(include, value, TryParseNullableDouble,
            result => result == null || !double.IsNaN(result.Value) && double.IsFinite(result.Value) && (include || result > double.MinValue), out bound);
    }

    public static bool TryParseFrom(bool include, string value, out RangeFilterBound<DateTime> bound)
    {
        return TryParse(include, value, TryParseNullableDateTime, _ => true, out bound);
    }

    public static bool TryParseTo(bool include, string value, out RangeFilterBound<double> bound)
    {
        return TryParse(include, value, TryParseNullableDouble,
            result => result == null || !double.IsNaN(result.Value) && double.IsFinite(result.Value) && (!include || result < double.MaxValue), out bound);
    }

    public static bool TryParseTo(bool include, string value, out RangeFilterBound<DateTime> bound)
    {
        return TryParse(include, value, TryParseNullableDateTime, _ => true, out bound);
    }

    private static bool TryParse<T>(bool include, string value, TryParseDelegate<T?> tryParse, Func<T?, bool> isValid, out RangeFilterBound<T> bound)
        where T: struct
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

    private static bool TryParseNullableDouble(string value, out double? result)
    {
        var success = TryParseNullable(value, TryParseDouble, out result);
        return success;
    }

    private static bool TryParseDouble(string value, out double result) =>
        double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);

    private static bool TryParseNullableDateTime(string value, out DateTime? result)
    {
        var success = TryParseNullable(value, TryParseDateTime, out result);
        return success;
    }

    private static bool TryParseDateTime(string value, out DateTime result) =>
        DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result);

    private static bool TryParseNullable<T>(string value, TryParseDelegate<T> tryParse, out T? result)
        where T: struct
    {
        if (value != null)
        {
            var success = tryParse(value, out var parsed);
            result = parsed;
            return success;
        }

        result = null;
        return true;
    }
}
