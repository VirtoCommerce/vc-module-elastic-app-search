using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Core.Services.Builders
{
    public interface IAggregationsResponseBuilder
    {
        IList<AggregationResponse> ToAggregationResult(IList<SearchResultWrapper> searchResults, IList<AggregationRequest> aggregations);
    }
}
