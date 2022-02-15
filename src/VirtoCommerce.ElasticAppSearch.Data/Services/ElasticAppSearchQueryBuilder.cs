using VirtoCommerce.ElasticAppSearch.Data.Models.Search;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchQueryBuilder
{
    public virtual SearchQuery ToSearchQuery(SearchRequest request)
    {
        var searchQuery = new SearchQuery
        {
            Query = string.Empty,
            Page = new SearchQueryPage
            {
                Current = (request.Skip / request.Take) + 1,
                Size = request.Take
            }
        };
        return searchQuery;
    }
}
