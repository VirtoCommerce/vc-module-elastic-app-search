using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Data.Services.Builders;

public class SearchResponseBuilder : ISearchResponseBuilder
{
    private readonly IDocumentConverter _documentConverter;
    private readonly IFieldNameConverter _fieldNameConverter;
    private readonly IAggregationsResponseBuilder _aggregationsResponseBuilder;

    public SearchResponseBuilder(IDocumentConverter documentConverter,
        IFieldNameConverter fieldNameConverter,
        IAggregationsResponseBuilder aggregationsResponseBuilder)
    {
        _documentConverter = documentConverter;
        _fieldNameConverter = fieldNameConverter;
        _aggregationsResponseBuilder = aggregationsResponseBuilder;
    }

    public virtual SearchResponse ToSearchResponse(SearchResult searchResult)
    {
        var searchResponse = new SearchResponse
        {
            Documents = searchResult.Results.Select(_documentConverter.ToSearchDocument).ToList(),
            TotalCount = searchResult.Meta.Page.TotalResults
        };
        return searchResponse;
    }

    public SearchResponse ToSearchResponse(IList<SearchResultWrapper> searchResults, IList<AggregationRequest> aggregations)
    {
        // create request based on main request
        var searchResult = searchResults.FirstOrDefault().SearchResult;

        var searchResponse = new SearchResponse
        {
            Documents = searchResult.Results.Select(_documentConverter.ToSearchDocument).ToList(),
            TotalCount = searchResult.Meta.Page.TotalResults,
            Aggregations = ToAggregationResult(searchResults, aggregations),
        };
        return searchResponse;
    }

    protected virtual IList<AggregationResponse> ToAggregationResult(IList<SearchResultWrapper> searchResults, IList<AggregationRequest> aggregations)
    {
        return _aggregationsResponseBuilder.ToAggregationResult(searchResults, aggregations);
    }
}
