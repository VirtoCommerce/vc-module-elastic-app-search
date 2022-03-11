using System;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

public abstract record RangeFilter<TValue> : Filter<RangeFilterValue<TValue>>
    where TValue: struct
{
    public sealed override string FieldName { get; init; }

    public sealed override RangeFilterValue<TValue> Value { get; init; }

    protected RangeFilter()
    {
    }

    protected RangeFilter(
        string fieldName,
        RangeFilterBound<TValue> from, Func<TValue?, TValue?> fromExcludeConverter,
        RangeFilterBound<TValue> to, Func<TValue?, TValue?> toIncludeConverter)
    {
        FieldName = fieldName;
        Value = new RangeFilterValue<TValue>
        {
            From = from.Include ? from.Value : fromExcludeConverter(from.Value),
            To = to.Include ? toIncludeConverter(to.Value) : to.Value
        };
    }
}
