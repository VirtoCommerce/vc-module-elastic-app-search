namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record Page
{
    public int Current { get; init; }

    public int TotalPages { get; init; }

    public int TotalResults { get; init; }

    public int Size { get; init; }
}
