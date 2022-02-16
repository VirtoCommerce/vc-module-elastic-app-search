namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

public record SearchResultPage
{
    public int Current { get; set; }

    public int TotalPages { get; set; }
    
    public int TotalResults { get; set; }

    public int Size { get; set; }
}
