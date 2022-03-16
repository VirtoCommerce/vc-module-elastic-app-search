namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;

public record Page
{
    public int Size { get; set; }

    public int Current { get; set; }
};
