namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

public record RangeFilterValue<TValue>
    where TValue : struct
{
    public TValue? From { get; init; }

    public TValue? To { get; init; }
}
