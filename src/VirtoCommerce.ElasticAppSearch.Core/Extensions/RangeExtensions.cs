using System;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class RangeExtensions
{
    private delegate bool TryParseDelegate<TValue>(bool include, string value, out RangeBound<TValue> result) where TValue : struct;

    public static bool TryParse(string fieldName, bool includeFrom, string fromValue, bool includeTo, string toValue, out NumberRangeFilter result)
    {
        return TryParse<NumberRangeFilter, double>(
            includeFrom, fromValue, RangeBoundExtensions.TryParseFrom,
            includeTo, toValue, RangeBoundExtensions.TryParseTo,
            (from, to) => new NumberRangeFilter(fieldName, from, to), out result);
    }

    public static bool TryParse(string fieldName, bool includeFrom, string fromValue, bool includeTo, string toValue, out DateTimeRangeFilter result)
    {
        return TryParse<DateTimeRangeFilter, DateTime>(
            includeFrom, fromValue, RangeBoundExtensions.TryParseFrom,
            includeTo, toValue, RangeBoundExtensions.TryParseTo,
            (from, to) => new DateTimeRangeFilter(fieldName, from, to), out result);
    }

    public static bool TryParse(string ragneName, bool includeFrom, string fromValue, bool includeTo, string toValue, out FacetRangeValue<double> result)
    {
        return TryParse<FacetRangeValue<double>, double>(
            includeFrom, fromValue, RangeBoundExtensions.TryParseFrom,
            includeTo, toValue, RangeBoundExtensions.TryParseTo,
            (from, to) => new FacetRangeValue<double>()
            {
                Name = ragneName,
                From = from.Include ? from.Value : from.Value?.GetNearestLower(),
                To = to.Include ? to.Value : to.Value?.GetNearestHigher(),
            },
            out result);
    }

    public static bool TryParse(string ragneName, bool includeFrom, string fromValue, bool includeTo, string toValue, out FacetRangeValue<DateTime> result)
    {
        return TryParse<FacetRangeValue<DateTime>, DateTime>(
            includeFrom, fromValue, RangeBoundExtensions.TryParseFrom,
            includeTo, toValue, RangeBoundExtensions.TryParseTo,
            (from, to) => new FacetRangeValue<DateTime>()
            {
                Name = ragneName,
                From = from.Include ? from.Value : from.Value?.GetPreviousSecond(),
                To = to.Include ? to.Value?.GetNextSecond() : to.Value,
            },
            out result);
    }

    private static bool TryParse<TFilter, TValue>(
        bool includeFrom, string fromValue, TryParseDelegate<TValue> tryParseTo,
        bool includeTo, string toValue, TryParseDelegate<TValue> tryParseFrom,
        Func<RangeBound<TValue>, RangeBound<TValue>, TFilter> filterFactory, out TFilter result)
        where TFilter : class
        where TValue : struct
    {
        var isValidFromBound = tryParseFrom(includeFrom, fromValue, out var from);
        var isValidToBound = tryParseTo(includeTo, toValue, out var to);
        var isSuccessful = isValidFromBound && isValidToBound;
        result = isSuccessful ? filterFactory(from, to) : null;
        return isSuccessful;
    }
}
