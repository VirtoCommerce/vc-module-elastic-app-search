namespace VirtoCommerce.ElasticAppSearch.Data.Models.Search;

public record SearchQueryPage
{
    public int Size { get; set; }

    public int Current { get; set; }
};
