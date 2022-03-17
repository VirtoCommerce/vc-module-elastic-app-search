using System.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Data.Services.Builders;

public class SearchResponseBuilder : ISearchResponseBuilder
{
    private readonly IDocumentConverter _documentConverter;

    public SearchResponseBuilder(IDocumentConverter documentConverter)
    {
        _documentConverter = documentConverter;
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
}
