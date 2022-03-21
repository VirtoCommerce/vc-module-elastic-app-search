namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record FacetResult
{
    public string Type { get; init; }

    public string Name { get; init; }

    public FacetData[] Data { get; init; }
}
