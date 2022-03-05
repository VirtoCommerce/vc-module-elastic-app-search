namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.CombiningFilters;

public interface ICombiningFilter
{
    public IFilter[] Value { get; init; }
}
