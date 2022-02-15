namespace VirtoCommerce.ElasticAppSearch.Data.Models.Search;

public record SearchResultDocumentMetadata
{
    public string Engine { get; set; }

    public double Score { get; set; }

    public string Id { get; set; }
}
