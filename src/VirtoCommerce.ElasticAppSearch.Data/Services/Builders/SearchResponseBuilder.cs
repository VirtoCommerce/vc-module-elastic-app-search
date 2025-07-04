using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Suggestions;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.ElasticAppSearch.Data.Extensions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Data.Services.Builders;

public class SearchResponseBuilder : ISearchResponseBuilder
{
    private readonly IDocumentConverter _documentConverter;
    private readonly IAggregationsResponseBuilder _aggregationsResponseBuilder;

    public SearchResponseBuilder(IDocumentConverter documentConverter,
        IAggregationsResponseBuilder aggregationsResponseBuilder)
    {
        _documentConverter = documentConverter;
        _aggregationsResponseBuilder = aggregationsResponseBuilder;
    }

    public virtual SearchResponse ToSearchResponse(SearchResult searchResult)
    {
        var searchResponse = OverridableType<SearchResponse>.New();
        searchResponse.Documents = searchResult.Results.Select(_documentConverter.ToSearchDocument).ToList();
        searchResponse.TotalCount = searchResult.Meta.Page.TotalResults;
        searchResponse.Aggregations = _aggregationsResponseBuilder.ToAggregationResult(searchResult);

        return searchResponse;
    }

    public virtual SearchResponse ToSearchResponse(IList<SearchResultAggregationWrapper> searchResults, IList<AggregationRequest> aggregations)
    {
        var searchResponse = OverridableType<SearchResponse>.New();

        // create request based on main request
        var searchResult = searchResults?.FirstOrDefault()?.SearchResult;
        if (searchResult == null)
        {
            return searchResponse;
        }

        searchResponse.Documents = searchResult.Results.Select(_documentConverter.ToSearchDocument).ToList();
        searchResponse.TotalCount = searchResult.Meta.Page.TotalResults;
        searchResponse.Aggregations = _aggregationsResponseBuilder.ToAggregationResult(searchResults, aggregations);

        return searchResponse;
    }

    public virtual SuggestionResponse ToSuggestionResponse(SuggestionApiResponse apiResponse)
    {
        var response = new SuggestionResponse
        {
            Suggestions = apiResponse.Results.Documents.Select(x => x.Suggestion).ToArray(),
        };

        return response;
    }
}
