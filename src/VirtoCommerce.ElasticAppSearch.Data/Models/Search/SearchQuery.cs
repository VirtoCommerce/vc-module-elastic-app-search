namespace VirtoCommerce.ElasticAppSearch.Data.Models.Search;

public record SearchQuery
{
    public string Query { get; set; }

    public SearchQueryPage Page { get; set; }
}
