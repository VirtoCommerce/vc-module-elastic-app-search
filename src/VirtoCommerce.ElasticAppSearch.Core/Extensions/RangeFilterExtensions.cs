using System;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class RangeFilterExtensions
{
    private delegate bool TryParseDelegate<TValue>(bool include, string value, out RangeFilterBound<TValue> result);

    public static bool TryParse(string fieldName, bool includeFrom, string fromValue, bool includeTo, string toValue, out DecimalRangeFilter result)
    {
        return TryParse<DecimalRangeFilter, decimal>(
            includeFrom, fromValue, RangeFilterBoundExtensions.TryParseFrom,
            includeTo, toValue, RangeFilterBoundExtensions.TryParseTo,
            (from, to) => new DecimalRangeFilter(fieldName, from, to), out result);
    }

    public static bool TryParse(string fieldName, bool includeFrom, string fromValue, bool includeTo, string toValue, out DoubleRangeFilter result)
    {
        return TryParse<DoubleRangeFilter, double>(
            includeFrom, fromValue, RangeFilterBoundExtensions.TryParseFrom,
            includeTo, toValue, RangeFilterBoundExtensions.TryParseTo,
            (from, to) => new DoubleRangeFilter(fieldName, from, to), out result);
    }

    public static bool TryParse(string fieldName, bool includeFrom, string fromValue, bool includeTo, string toValue, out DateTimeRangeFilter result)
    {
        return TryParse<DateTimeRangeFilter, DateTime>(
            includeFrom, fromValue, RangeFilterBoundExtensions.TryParseFrom,
            includeTo, toValue, RangeFilterBoundExtensions.TryParseTo,
            (from, to) => new DateTimeRangeFilter(fieldName, from, to), out result);
    }

    private static bool TryParse<TFilter, TValue>(
        bool includeFrom, string fromValue, TryParseDelegate<TValue> tryParseTo,
        bool includeTo, string toValue, TryParseDelegate<TValue> tryParseFrom,
        Func< RangeFilterBound<TValue>, RangeFilterBound<TValue>, TFilter> filterFactory, out TFilter result)
        where TFilter: class
    {
        var isValidFromBound = tryParseFrom(includeFrom, fromValue, out var from);
        var isValidToBound = tryParseTo(includeTo, toValue, out var to);
        var isSuccessful = isValidFromBound && isValidToBound;
        result = isSuccessful ? filterFactory(from, to) : null;
        return isSuccessful;
    }
}
