namespace VirtoCommerce.ElasticAppSearch.Data.Models;

public record DeleteDocumentResult
{
    public string Id { get; set; }

    public bool Deleted { get; set; }
}
