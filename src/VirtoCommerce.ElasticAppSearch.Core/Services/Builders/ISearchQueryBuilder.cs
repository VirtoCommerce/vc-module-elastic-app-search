using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Core.Services.Builders;

public interface ISearchQueryBuilder
{
    SearchQuery ToSearchQuery(SearchRequest request, Schema schema);

    IList<SearchQuery> ToSearchQueries(SearchRequest request, Schema schema);
}
