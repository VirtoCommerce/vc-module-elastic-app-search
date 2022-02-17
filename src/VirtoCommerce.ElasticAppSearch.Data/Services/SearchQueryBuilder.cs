using System;
using System.Linq;
using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class SearchQueryBuilder : ISearchQueryBuilder
{
    private readonly IFieldNameConverter _fieldNameConverter;

    public SearchQueryBuilder(IFieldNameConverter fieldNameConverter)
    {
        _fieldNameConverter = fieldNameConverter;
    }

    public virtual SearchQuery ToSearchQuery(SearchRequest request)
    {
        if (request.IsFuzzySearch)
        {
            throw new NotSupportedException("Fuzzy search is not supported by Elastic App Search provider. Please use the Precision Tuning feature, which is part of Relevance Tuning, instead.");
        }

        var searchQuery = new SearchQuery
        {
            Query = request.SearchKeywords ?? string.Empty,
            Sort = GetSorting(request?.Sorting),
            Page = new SearchQueryPage
            {
                Current = (request.Skip / request.Take) + 1,
                Size = request.Take
            }
        };

        return searchQuery;
    }

    protected virtual IList<SearchQuerySortField> GetSorting(IEnumerable<SortingField> sortingFields)
    {
        var result = sortingFields?
            .Select(x => new SearchQuerySortField()
            {
                Field = _fieldNameConverter.ToProviderFieldName(x.FieldName),
                Order = x.IsDescending ? "desc" : "asc"
            })
            .ToList();

        return result;
    }
}
