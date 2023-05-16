using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Suggestions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Core.Services.Builders;

public interface ISearchQueryBuilder
{
    IList<SearchQueryAggregationWrapper> ToSearchQueries(SearchRequest request, Schema schema);

    SuggestionApiQuery ToSuggestionQuery(SuggestionRequest request);
}
