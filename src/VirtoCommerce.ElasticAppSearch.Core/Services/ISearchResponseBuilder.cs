using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Core.Services;

public interface ISearchResponseBuilder
{
    SearchResponse ToSearchResponse(SearchResult searchResult);
}
