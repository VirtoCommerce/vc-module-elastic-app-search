namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.CombiningFilters;

public sealed record NoneFilter : Filter<IFilter[]>, ICombiningFilter
{
    public override string FieldName => "none";

    public override IFilter[] Value { get; init; }
}
