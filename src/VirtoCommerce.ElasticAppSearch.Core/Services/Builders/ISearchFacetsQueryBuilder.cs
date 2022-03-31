using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Core.Services.Builders
{
    public interface ISearchFacetsQueryBuilder
    {
        IList<FacetRequest> GetFacetRequests(IEnumerable<AggregationRequest> aggregations, Schema schema);
    }
}
