namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

public record SearchQueryPage
{
    public int Size { get; set; }

    public int Current { get; set; }
};
