using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    private readonly IFieldNameConverter _fieldNameConverter;
    private readonly ISearchFiltersBuilder _searchFiltersBuilder;
    private readonly ISearchFacetsQueryBuilder _facetsBuilder;

    public SearchQueryBuilder(IFieldNameConverter fieldNameConverter,
        ISearchFiltersBuilder searchFiltersBuilder,
        ISearchFacetsQueryBuilder facetsBuilder)
    {
        _fieldNameConverter = fieldNameConverter;
        _searchFiltersBuilder = searchFiltersBuilder;
        _facetsBuilder = facetsBuilder;
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
            Sort = GetSorting(request.Sorting, schema),
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

    public IList<SearchQueryAggregationWrapper> ToSearchQueries(SearchRequest request, Schema schema)
    {
        var mainSearchQuery = ToSearchQuery(request, schema);
        var mainSearchQueryWrapper = new SearchQueryAggregationWrapper
        {
            SearchQuery = mainSearchQuery
        };
        var queries = new List<SearchQueryAggregationWrapper> { mainSearchQueryWrapper };

        var facetRequests = GetFacets(request.Aggregations, schema);

        foreach (var filterGroup in facetRequests.GroupBy(x => x.FilterName))
        {
            if (filterGroup.Key == request.Filter?.ToString())
            {
                // add base request with facets
                mainSearchQuery.Facets = new Facets(filterGroup
                    .Where(x => x.Facet != null)
                    .ToDictionary(x => x.FacetFieldName, x => x.Facet));

                foreach (var facetRequest in filterGroup.Where(x => x.FieldName == null))
                {
                    // add filter request
                    var wrapper = new SearchQueryAggregationWrapper
                    {
                        AggregationId = facetRequest.Id,
                        SearchQuery = new SearchQuery
                        {
                            Query = request.SearchKeywords ?? string.Empty,
                            Filters = facetRequest.Filter,
                            SearchFields = GetSearchFields(request.SearchFields),
                            Page = new Page { Current = 1 }
                        }
                    };
                    queries.Add(wrapper);
                }
            }
            else
            {
                foreach (var fieldGroup in filterGroup.GroupBy(x => x.FieldName))
                {
                    if (string.IsNullOrEmpty(fieldGroup.Key))
                    {
                        foreach (var facetRequest in fieldGroup)
                        {
                            // add filter request
                            var wrapper = new SearchQueryAggregationWrapper
                            {
                                AggregationId = facetRequest.Id,
                                SearchQuery = new SearchQuery
                                {
                                    Query = request.SearchKeywords ?? string.Empty,
                                    Filters = facetRequest.Filter,
                                    SearchFields = GetSearchFields(request.SearchFields),
                                    Page = new Page { Current = 1 }
                                }
                            };

                            queries.Add(wrapper);
                        }
                    }
                    else
                    {
                        // add inverted filter request for selected aggregations
                        var invertedFacets = new Facets(filterGroup
                            .Where(x => x.Facet != null)
                            .ToDictionary(x => x.FacetFieldName, x => x.Facet));

                        var filter = filterGroup.FirstOrDefault().Filter;

                        var wrapper = new SearchQueryAggregationWrapper
                        {
                            SearchQuery = new SearchQuery
                            {
                                Query = request.SearchKeywords ?? string.Empty,
                                Filters = filter,
                                Facets = invertedFacets,
                                SearchFields = GetSearchFields(request.SearchFields),
                                Page = new Page { Current = 1 }
                            }
                        };

                        queries.Add(wrapper);
                    }
                }
            }
        }

        return queries;
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
        var result = searchFields?.ToDictionary(searchField => _fieldNameConverter.ToProviderFieldName(searchField), _ => new SearchFieldValue());

        return result;
    }

    protected virtual IFilters GetFilters(ISearchFilter filter, Schema schema)
    {
        return _searchFiltersBuilder.ToFilters(filter, schema);
    }

    protected virtual IList<FacetRequest> GetFacets(IList<AggregationRequest> aggregations, Schema schema)
    {
        return _facetsBuilder.GetFacets(aggregations, schema);
    }
}
