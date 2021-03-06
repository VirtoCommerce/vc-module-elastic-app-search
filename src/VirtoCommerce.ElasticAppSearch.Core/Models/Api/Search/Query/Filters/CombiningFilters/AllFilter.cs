namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.CombiningFilters;

public sealed record AllFilter : Filter<IFilter[]>, ICombiningFilter
{
    public override string FieldName => "all";

    public override IFilter[] Value { get; init; }
}
