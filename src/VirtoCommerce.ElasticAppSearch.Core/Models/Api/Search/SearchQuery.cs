namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

public record SearchQuery
{
    public string Query { get; set; }

    public SearchQueryPage Page { get; set; }
}
