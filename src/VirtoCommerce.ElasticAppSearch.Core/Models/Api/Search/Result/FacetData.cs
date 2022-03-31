namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record FacetData
{
    public string Name { get; init; }

    public object Value { get; init; }

    public object From { get; init; }

    public object To { get; init; }

    public int? Count { get; init; }
}
