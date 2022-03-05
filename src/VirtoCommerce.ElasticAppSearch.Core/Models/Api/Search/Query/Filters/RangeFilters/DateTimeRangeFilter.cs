using System;
using VirtoCommerce.ElasticAppSearch.Core.Extensions;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

public record DateTimeRangeFilter : RangeFilter<DateTime>
{

    public DateTimeRangeFilter(string fieldName, RangeFilterBound<DateTime> from, RangeFilterBound<DateTime> to):
        base(fieldName, from, fromValue => fromValue.GetPreviousSecond(), to, toValue => toValue.GetNextSecond())
    {
    }
}
