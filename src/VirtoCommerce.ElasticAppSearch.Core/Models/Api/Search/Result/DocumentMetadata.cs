namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record DocumentMetadata
{
    public string Engine { get; init; }

    public double? Score { get; init; }

    public string Id { get; init; }
}
