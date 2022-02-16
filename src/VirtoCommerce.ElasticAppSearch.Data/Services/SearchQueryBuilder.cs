using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class SearchQueryBuilder : ISearchQueryBuilder
{
    public virtual SearchQuery ToSearchQuery(SearchRequest request)
    {
        var searchQuery = new SearchQuery
        {
            Query = request.SearchKeywords ?? string.Empty,
            Page = new SearchQueryPage
            {
                Current = (request.Skip / request.Take) + 1,
                Size = request.Take
            }
        };
        return searchQuery;
    }
}
