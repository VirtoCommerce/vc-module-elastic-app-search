namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;

public record DeleteDocumentResult
{
    public string Id { get; init; }

    public bool Deleted { get; init; }
}
