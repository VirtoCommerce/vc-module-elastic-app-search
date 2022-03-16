namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record FieldValue
{
    public object Raw { get; init; }

    public string Snippet { get; init; }
};
