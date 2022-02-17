namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search
{
    public record SearchQuerySortField
    {
        public string Field { get; set; }

        public SearchQuerySortOrder Order { get; set; }
    }
}
