namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.CombiningFilters;

public sealed record AnyFilter: Filter<IFilter[]>
{
    public override string FieldName => "any";

    public override IFilter[] Value { get; init; }
}
