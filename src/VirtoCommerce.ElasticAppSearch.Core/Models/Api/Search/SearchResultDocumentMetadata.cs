namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

public record SearchResultDocumentMetadata
{
    public string Engine { get; init; }

    public double Score { get; init; }

    public string Id { get; init; }
}
