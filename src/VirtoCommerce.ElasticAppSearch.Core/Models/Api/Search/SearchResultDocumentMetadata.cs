namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

public record SearchResultDocumentMetadata
{
    public string Engine { get; set; }

    public double Score { get; set; }

    public string Id { get; set; }
}
