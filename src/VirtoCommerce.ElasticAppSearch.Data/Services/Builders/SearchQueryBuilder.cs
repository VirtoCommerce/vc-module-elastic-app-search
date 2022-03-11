using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.SearchFields;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sort;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.SearchModule.Core.Model;
using ISearchFilter = VirtoCommerce.SearchModule.Core.Model.IFilter;

namespace VirtoCommerce.ElasticAppSearch.Data.Services.Builders;

public class SearchQueryBuilder : ISearchQueryBuilder
{
    private readonly IFieldNameConverter _fieldNameConverter;
    private readonly ISearchFiltersBuilder _searchFiltersBuilder;

    public SearchQueryBuilder(IFieldNameConverter fieldNameConverter, ISearchFiltersBuilder searchFiltersBuilder)
    {
        _fieldNameConverter = fieldNameConverter;
        _searchFiltersBuilder = searchFiltersBuilder;
    }

    public virtual SearchQuery ToSearchQuery(SearchRequest request, Schema schema)
    {
        if (request.IsFuzzySearch)
        {
            Debug.WriteLine("Fuzzy search is not supported by Elastic App Search provider. Please use the Precision Tuning feature, which is part of Relevance Tuning, instead.");
        }

        var searchQuery = new SearchQuery
        {
            Query = request.SearchKeywords ?? string.Empty,
            Sort = GetSorting(request.Sorting),
            Filters = GetFilters(request.Filter, schema),
            SearchFields = GetSearchFields(request.SearchFields),
            Page = new Page
            {
                Current = request.Skip / request.Take + 1,
                Size = request.Take
            },
        };

        return searchQuery;
    }

    protected virtual Sort GetSorting(IEnumerable<SortingField> sortingFields)
    {
        var result = sortingFields != null
            ? new Sort(sortingFields
                .Select(sortingField => new Field<SortOrder>
                {
                    FieldName = _fieldNameConverter.ToProviderFieldName(sortingField.FieldName),
                    Value = sortingField.IsDescending ? SortOrder.Desc : SortOrder.Asc
                })
                .ToArray())
            : null;

        return result;
    }

    protected virtual SearchFields GetSearchFields(IEnumerable<string> searchFields)
    {
        var result = searchFields != null
            ? new SearchFields(searchFields.ToDictionary(
                searchField => _fieldNameConverter.ToProviderFieldName(searchField),
                _ => new SearchFieldValue()
            ))
            : null;

        return result;
    }

    protected virtual IFilters GetFilters(ISearchFilter filter, Schema schema)
    {
        return _searchFiltersBuilder.ToFilters(filter, schema);
    }
}
