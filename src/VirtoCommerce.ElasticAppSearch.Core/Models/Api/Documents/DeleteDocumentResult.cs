namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;

public record DeleteDocumentResult
{
    public string Id { get; set; }

    public bool Deleted { get; set; }
}
