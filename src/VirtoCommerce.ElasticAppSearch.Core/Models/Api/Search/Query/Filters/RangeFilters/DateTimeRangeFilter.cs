using System;
using VirtoCommerce.ElasticAppSearch.Core.Extensions;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

public record DateTimeRangeFilter : RangeFilter<DateTime>
{
    public DateTimeRangeFilter()
    {
    }

    public DateTimeRangeFilter(string fieldName, RangeBound<DateTime> from, RangeBound<DateTime> to) :
        base(fieldName, from, fromValue => fromValue?.GetNextMillisecond(), to, toValue => toValue?.GetNextMillisecond())
    {
    }
}
