using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.SearchModule.Core.Model;
using ISearchFilter = VirtoCommerce.SearchModule.Core.Model.IFilter;

namespace VirtoCommerce.ElasticAppSearch.Data.Services.Builders;

public class SearchQueryBuilder : ISearchQueryBuilder
{
    private readonly ILogger<SearchQueryBuilder> _logger;
    private readonly IFieldNameConverter _fieldNameConverter;
    private readonly ISearchFiltersBuilder _searchFiltersBuilder;
    private readonly ISearchFacetsQueryBuilder _facetsBuilder;

    public SearchQueryBuilder(ILogger<SearchQueryBuilder> logger,
        IFieldNameConverter fieldNameConverter,
        ISearchFiltersBuilder searchFiltersBuilder,
        ISearchFacetsQueryBuilder facetsBuilder)
    {
        _logger = logger;
        _fieldNameConverter = fieldNameConverter;
        _searchFiltersBuilder = searchFiltersBuilder;
        _facetsBuilder = facetsBuilder;
    }

    public IList<SearchQueryAggregationWrapper> ToSearchQueries(SearchRequest request, Schema schema)
    {
        var mainSearchQuery = ToSearchQuery(request, schema);
        var mainSearchQueryWrapper = new SearchQueryAggregationWrapper { SearchQuery = mainSearchQuery };
        var result = new List<SearchQueryAggregationWrapper> { mainSearchQueryWrapper };

        // process aggregates
        var facetRequests = GetFacetRequests(request.Aggregations, schema);

        foreach (var filterGroup in facetRequests?.GroupBy(x => x.FilterName) ?? new List<IGrouping<string, FacetRequest>>())
        {
            if (filterGroup.Key == request.Filter?.ToString())
            {
                // add base request with facets
                mainSearchQuery.Facets = GetFacets(filterGroup);

                // add requests for selected filters
                var aggregationFilterQueries = ToSearchQueryAggregationWrappers(filterGroup.Where(x => x.FieldName == null), request);
                result.AddRange(aggregationFilterQueries);
            }
            else
            {
                foreach (var fieldGroup in filterGroup.GroupBy(x => x.FieldName))
                {
                    // add requests for inverted filters
                    if (string.IsNullOrEmpty(fieldGroup.Key))
                    {
                        var aggregationFilterQueries = ToSearchQueryAggregationWrappers(fieldGroup, request);
                        result.AddRange(aggregationFilterQueries);
                    }
                    else
                    {
                        // add requests for inverted facets
                        var invertedFacets = GetFacets(filterGroup);
                        var filter = filterGroup.FirstOrDefault().Filter;

                        var wrapper = new SearchQueryAggregationWrapper
                        {
                            SearchQuery = new SearchQuery
                            {
                                Filters = filter,
                                Facets = invertedFacets,
                                Query = request.SearchKeywords ?? string.Empty,
                                SearchFields = GetSearchFields(request.SearchFields),
                                Page = new Page { Current = 1 }
                            }
                        };

                        result.Add(wrapper);
                    }
                }
            }
        }

        return result;
    }

    protected virtual SearchQuery ToSearchQuery(SearchRequest request, Schema schema)
    {
        if (request.IsFuzzySearch)
        {
            _logger.LogWarning("Fuzzy search is not supported by Elastic App Search provider. Please use the Precision Tuning feature, which is part of Relevance Tuning, instead.");
        }

        var searchQuery = new SearchQuery
        {
            Query = request.SearchKeywords ?? string.Empty,
            Sort = GetSorting(request.Sorting, schema),
            Filters = GetFilters(request.Filter, schema),
            SearchFields = GetSearchFields(request.SearchFields),
            ResultFields = GetResultFields(request.IncludeFields, schema),
            Page = new Page
            {
                Current = request.Skip / request.Take + 1,
                Size = request.Take
            },
        };

        return searchQuery;
    }

    protected virtual Field<SortOrder>[] GetSorting(IEnumerable<SortingField> sortingFields, Schema schema)
    {
        var result = sortingFields?
            .Select(sortingField => new Field<SortOrder>
            {
                FieldName = _fieldNameConverter.ToProviderFieldName(sortingField.FieldName),
                Value = sortingField.IsDescending ? SortOrder.Desc : SortOrder.Asc
            })
            .Where(x => schema.Fields.ContainsKey(x.FieldName))
            .ToArray();

        return result;
    }

    protected virtual Dictionary<string, SearchFieldValue> GetSearchFields(IEnumerable<string> searchFields)
    {
        searchFields = searchFields?.Where(x => !ModuleConstants.Api.FieldNames.IgnoredForSearch.Contains(x));

        var result = searchFields?.ToDictionary(searchField => _fieldNameConverter.ToProviderFieldName(searchField), _ => new SearchFieldValue());

        return result;
    }

    protected virtual Dictionary<string, ResultFieldValue> GetResultFields(IEnumerable<string> includeFields, Schema schema)
    {
        var result = includeFields?
            .Select(x => _fieldNameConverter.ToProviderFieldName(x))
            .Where(x => schema.Fields.ContainsKey(x))
            .ToDictionary(x => x, _ => new ResultFieldValue());

        return result;
    }

    protected virtual IFilters GetFilters(ISearchFilter filter, Schema schema)
    {
        return _searchFiltersBuilder.ToFilters(filter, schema);
    }


    private IList<FacetRequest> GetFacetRequests(IList<AggregationRequest> aggregations, Schema schema)
    {
        return _facetsBuilder.GetFacetRequests(aggregations, schema);
    }

    private static Dictionary<string, Facet> GetFacets(IEnumerable<FacetRequest> facetRequests)
    {
        return facetRequests
            .Where(x => x.Facet != null)
            .ToDictionary(x => x.FacetFieldName, x => x.Facet);
    }

    private IEnumerable<SearchQueryAggregationWrapper> ToSearchQueryAggregationWrappers(IEnumerable<FacetRequest> facets, SearchRequest request)
    {
        return facets.Select(x => new SearchQueryAggregationWrapper
        {
            AggregationId = x.Id,
            SearchQuery = new SearchQuery
            {
                Filters = x.Filter,
                Query = request.SearchKeywords ?? string.Empty,
                SearchFields = GetSearchFields(request.SearchFields),
                Page = new Page { Current = 1 }
            }
        });
    }
}
