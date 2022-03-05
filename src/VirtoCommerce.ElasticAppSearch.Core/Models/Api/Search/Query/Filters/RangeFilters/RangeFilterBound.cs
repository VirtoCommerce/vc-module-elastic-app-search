namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

public record RangeFilterBound<T>
    where T : notnull
{
    public bool Include { get; set; }

    public T Value { get; init; }
}
