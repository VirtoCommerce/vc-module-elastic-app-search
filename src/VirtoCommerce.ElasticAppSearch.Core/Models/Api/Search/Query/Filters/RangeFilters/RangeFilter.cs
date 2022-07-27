using System;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

public abstract record RangeFilter<TValue> : Filter<RangeValue<TValue>>
    where TValue : struct
{
    public sealed override string FieldName { get; init; }

    public sealed override RangeValue<TValue> Value { get; init; }

    protected RangeFilter()
    {
    }

    protected RangeFilter(
        string fieldName,
        RangeBound<TValue> from, Func<TValue?, TValue?> fromExcludeConverter,
        RangeBound<TValue> to, Func<TValue?, TValue?> toIncludeConverter)
    {
        FieldName = fieldName;
        Value = new RangeValue<TValue>
        {
            From = from.Include ? from.Value : fromExcludeConverter(from.Value),
            To = to.Include ? to.Value : toIncludeConverter(to.Value)
        };
    }
}
