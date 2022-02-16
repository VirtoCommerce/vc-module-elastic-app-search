namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

public record SearchResultPage
{
    public int Current { get; init; }

    public int TotalPages { get; init; }
    
    public int TotalResults { get; init; }

    public int Size { get; init; }
}
