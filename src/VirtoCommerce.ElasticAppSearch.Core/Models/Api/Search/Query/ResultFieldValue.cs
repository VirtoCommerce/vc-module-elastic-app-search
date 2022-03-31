namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;

public record ResultFieldValue
{
    public ResultFieldRaw Raw { get; init; } = new ResultFieldRaw();
}
