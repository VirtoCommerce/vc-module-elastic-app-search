namespace VirtoCommerce.ElasticAppSearch.Data.Models.Documents;

public record DeleteDocumentResult
{
    public string Id { get; set; }

    public bool Deleted { get; set; }
}
