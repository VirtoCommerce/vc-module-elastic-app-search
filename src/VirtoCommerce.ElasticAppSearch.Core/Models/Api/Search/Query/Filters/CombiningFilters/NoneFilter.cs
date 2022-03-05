namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.CombiningFilters;

public sealed record NoneFilter : Filter<IFilter[]>
{
    public override string FieldName { get; init; } = "none";

    public override IFilter[] Value { get; init; }
}
