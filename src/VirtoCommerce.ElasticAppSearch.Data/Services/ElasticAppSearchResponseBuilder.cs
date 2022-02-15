using System.Linq;
using VirtoCommerce.ElasticAppSearch.Data.Models.Search;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchResponseBuilder
{
    public virtual SearchResponse ToSearchResponse(SearchResult searchResult)
    {
        var searchResponse = new SearchResponse
        {
            Documents = searchResult.Results.Select(ToSearchDocument).ToList(),
            TotalCount = searchResult.Meta.Page.TotalResults
        };
        return searchResponse;
    }

    protected virtual SearchDocument ToSearchDocument(SearchResultDocument document)
    {
        var searchDocument = new SearchDocument { Id = document.Id };
        foreach (var (fieldName, value) in document.Fields)
        {
            searchDocument.Add(fieldName, value);
        }

        return searchDocument;
    }
}
