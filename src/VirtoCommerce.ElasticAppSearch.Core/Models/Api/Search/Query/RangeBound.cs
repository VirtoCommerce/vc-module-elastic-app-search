namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;

public record RangeBound<T>
    where T : struct
{
    public bool Include { get; set; }

    public T? Value { get; init; }
}
