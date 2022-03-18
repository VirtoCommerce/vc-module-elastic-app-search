using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Core.Services.Builders;

public interface ISearchResponseBuilder
{
    SearchResponse ToSearchResponse(SearchResult searchResult);

    SearchResponse ToSearchResponse(IList<SearchResultWrapper> searchResults, IList<AggregationRequest> aggregations);
}
