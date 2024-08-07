using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Extensions;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sorting;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Suggestions;
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
    private readonly ISearchBoostsBuilder _boostsBuilder;

    public SearchQueryBuilder(ILogger<SearchQueryBuilder> logger,
        IFieldNameConverter fieldNameConverter,
        ISearchFiltersBuilder searchFiltersBuilder,
        ISearchFacetsQueryBuilder facetsBuilder,
        ISearchBoostsBuilder boostsBuilder)
    {
        _logger = logger;
        _fieldNameConverter = fieldNameConverter;
        _searchFiltersBuilder = searchFiltersBuilder;
        _facetsBuilder = facetsBuilder;
        _boostsBuilder = boostsBuilder;
    }

    public IList<SearchQueryAggregationWrapper> ToSearchQueries(SearchRequest request, Schema schema, SearchSettings searchSettings)
    {
        var mainSearchQuery = ToSearchQuery(request, schema, searchSettings);
        var mainSearchQueryWrapper = new SearchQueryAggregationWrapper { SearchQuery = mainSearchQuery };
        var result = new List<SearchQueryAggregationWrapper> { mainSearchQueryWrapper };

        // process aggregates
        var facetRequests = GetFacetRequests(request.Aggregations, schema);

        var groupedFilters = facetRequests?.GroupBy(x => x.FilterName) ?? new List<IGrouping<string, FacetRequest>>();
        foreach (var filterGroup in groupedFilters)
        {
            if (filterGroup.Key == request.Filter?.ToString())
            {
                // add base request with facets
                mainSearchQuery.Facets = GetFacets(filterGroup);

                // add requests for selected filters
                var aggregationFilterQueries = ToSearchQueryAggregationWrappers(filterGroup.Where(x => x.FieldName is null), request);
                result.AddRange(aggregationFilterQueries);
            }
            else
            {
                var filterGroups = filterGroup.GroupBy(x => x.FieldName);
                foreach (var fieldGroup in filterGroups)
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
                        var filter = filterGroup.First().Filter;

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

    public virtual SuggestionApiQuery ToSuggestionQuery(SuggestionRequest request)
    {
        var apiQuery = new SuggestionApiQuery
        {
            Query = request.Query,
            Size = request.Size,
        };

        var fields = request.Fields
            ?.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => _fieldNameConverter.ToProviderFieldName(x))
            .ToArray();

        if (fields?.Any() == true)
        {
            apiQuery.Types = new SuggestionsApiQueryType
            {
                Documents = new SuggestionsApiQueryTypeDocument
                {
                    Fields = fields,
                },
            };
        }

        return apiQuery;
    }


    protected virtual SearchQuery ToSearchQuery(SearchRequest request, Schema schema, SearchSettings settings)
    {
        if (request.IsFuzzySearch)
        {
            _logger.LogWarning("Fuzzy search is not supported by Elastic App Search provider. Please use the Precision Tuning feature, which is part of Relevance Tuning, instead");
        }

        var searchQuery = new SearchQuery
        {
            Query = request.SearchKeywords ?? string.Empty,
            Sort = GetSorting(request.Sorting, schema),
            Filters = GetFilters(request.Filter, schema),
            SearchFields = GetSearchFields(request.SearchFields),
            ResultFields = GetResultFields(request.IncludeFields, schema),
            Boosts = GetBoosts(request.Boosts, schema, settings),
            Page = new Page
            {
                Current = request.Take == 0 ? 1 : request.Skip / request.Take + 1,
                Size = request.Take
            }
        };

        return searchQuery;
    }

    protected virtual ISort[] GetSorting(IEnumerable<SortingField> sortingFields, Schema schema)
    {
        var result = sortingFields?.Select(GetSortingField)
            .Where(x => schema.Fields.ContainsKey(x.FieldName) || x.FieldName == ModuleConstants.Api.FieldNames.ScoreFieldName)
            .ToArray();

        return result;
    }

    protected virtual ISort GetSortingField(SortingField field)
    {
        ISort result;

        if (field is GeoDistanceSortingField geoSorting)
        {
            result = new GeoDistanceSort
            {
                FieldName = _fieldNameConverter.ToProviderFieldName(field.FieldName),
                Value = new GeoDistanceSortValue
                {
                    Center = geoSorting.Location.ToGeoPoint(),
                    Order = field.IsDescending ? SortOrder.Desc : SortOrder.Asc
                }
            };
        }
        else
        {
            result = new FieldSort
            {
                FieldName = _fieldNameConverter.ToProviderFieldName(field.FieldName),
                Value = field.IsDescending ? SortOrder.Desc : SortOrder.Asc
            };
        }

        return result;
    }

    protected virtual Dictionary<string, SearchFieldValue> GetSearchFields(IEnumerable<string> searchFields)
    {
        searchFields = searchFields?.Where(x => !ModuleConstants.Api.FieldNames.IgnoredForSearch.Any(ignoredField => x.StartsWith(ignoredField)));

        var result = searchFields?.ToDictionary(searchField => _fieldNameConverter.ToProviderFieldName(searchField), _ => new SearchFieldValue());

        return result;
    }

    protected virtual Dictionary<string, ResultFieldValue> GetResultFields(IList<string> includeFields, Schema schema)
    {
        // combine all __object properties into one property
        if (includeFields?.Any(x => x.StartsWith(ModuleConstants.Api.FieldNames.ObjectFieldName)) == true)
        {
            var newIncludeFields = includeFields.Where(x => !x.StartsWith(ModuleConstants.Api.FieldNames.ObjectFieldName)).ToList();
            newIncludeFields.Add(ModuleConstants.Api.FieldNames.ObjectFieldName);

            includeFields = newIncludeFields;
        }

        var result = includeFields?
            .Select(x => _fieldNameConverter.ToProviderFieldName(x))
            .Where(x => schema.Fields.ContainsKey(x))
            .ToDictionary(x => x, _ => new ResultFieldValue());

        return result;
    }


    protected virtual Dictionary<string, Boost[]> GetBoosts(IList<SearchBoost> boosts, Schema schema, SearchSettings settings)
    {
        return _boostsBuilder.ToBoosts(boosts, schema, settings);
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
        var result = new Dictionary<string, Facet>();

        foreach (var facetRequest in facetRequests.Where(x => x.Facet is not null))
        {
            result.TryAdd(facetRequest.FacetFieldName, facetRequest.Facet);
        }

        return result;
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
