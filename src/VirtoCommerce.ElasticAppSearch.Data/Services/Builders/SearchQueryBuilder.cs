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

    public IList<SearchQuery> ToSearchQueries(SearchRequest request, Schema schema)
    {
        var mainSearchQuery = ToSearchQuery(request, schema);

        var queries = new List<SearchQuery> { mainSearchQuery };

        var facetRequests = GetFacets(request.Aggregations, schema);

        var grouping = facetRequests.GroupBy(f => f.FilterName);
        foreach (var filterGroup in grouping)
        {
            if (filterGroup.Key == request.Filter?.ToString())
            {
                // add base request with facets
                mainSearchQuery.Facets = new Facets(
                    filterGroup
                    .Where(x => x.FacetFieldName != null) // TODO: field name with underscore
                    .ToDictionary(x => x.FacetFieldName, x => x.Facet));
            }
            else
            {
                foreach (var fieldGroup in filterGroup.GroupBy(f => f.FieldName))
                {
                    if (string.IsNullOrEmpty(fieldGroup.Key))
                    {
                        foreach (var facetRequest in fieldGroup)
                        {
                            // add filter request
                            var filterSearchQuery = new SearchQuery
                            {
                                AggregationId = facetRequest.Id,

                                Query = request.SearchKeywords ?? string.Empty,
                                Filters = facetRequest.Filter,
                                SearchFields = GetSearchFields(request.SearchFields),
                                Page = new Page
                                {
                                    Current = 1,
                                    Size = 0
                                }
                            };

                            queries.Add(filterSearchQuery);
                        }
                    }
                    else
                    {
                        // add inverted filter request for selected aggregations
                        var invertedFacets = new Facets(
                            filterGroup
                            .Where(x => x.FacetFieldName != null) // TODO: field name with underscore
                            .ToDictionary(x => x.FacetFieldName, x => x.Facet));

                        var filter = filterGroup.FirstOrDefault().Filter;

                        var filterSearchQuery = new SearchQuery
                        {
                            Query = request.SearchKeywords ?? string.Empty,
                            Filters = filter,
                            Facets = invertedFacets,
                            SearchFields = GetSearchFields(request.SearchFields),
                            Page = new Page
                            {
                                Current = 1,
                                Size = 0
                            }
                        };
                        queries.Add(filterSearchQuery);
                    }
                }
            }
        }

        return queries;
    }

    protected virtual Field<SortOrder>[] GetSorting(IEnumerable<SortingField> sortingFields)
    {
        var result = sortingFields?.Select(sortingField => new Field<SortOrder>
        {
            FieldName = _fieldNameConverter.ToProviderFieldName(sortingField.FieldName),
            Value = sortingField.IsDescending ? SortOrder.Desc : SortOrder.Asc
        }).ToArray();

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
