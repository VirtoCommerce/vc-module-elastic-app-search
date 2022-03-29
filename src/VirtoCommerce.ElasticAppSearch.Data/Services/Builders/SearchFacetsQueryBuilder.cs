using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Extensions;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Data.Services.Builders;

public class SearchFacetsQueryBuilder : ISearchFacetsQueryBuilder
{
    private const int MaxFacetValues = 250;

    private readonly IFieldNameConverter _fieldNameConverter;
    private readonly ISearchFiltersBuilder _searchFiltersBuilder;

    public SearchFacetsQueryBuilder(IFieldNameConverter fieldNameConverter, ISearchFiltersBuilder searchFiltersBuilder)
    {
        _fieldNameConverter = fieldNameConverter;
        _searchFiltersBuilder = searchFiltersBuilder;
    }

    public IList<FacetRequest> GetFacetRequests(IEnumerable<AggregationRequest> aggregations, Schema schema)
    {
        var results = new List<FacetRequest>();

        if (aggregations.IsNullOrEmpty())
        {
            return results;
        }

        aggregations = PreProcessAggregations(aggregations);

        foreach (var aggregation in aggregations)
        {
            Facet facet = null;

            switch (aggregation)
            {
                case TermAggregationRequest termAggregationRequest:
                    facet = AddTermAggregationRequest(termAggregationRequest, schema);
                    break;
                case RangeAggregationRequest rangeAggregationRequest:
                    facet = AddRangeAggregationRequest(rangeAggregationRequest, schema);
                    break;
            }

            if (aggregation.Filter != null || facet != null)
            {
                var facetRequest = new FacetRequest
                {
                    Id = aggregation.Id,
                    FieldName = aggregation.FieldName,
                    FacetFieldName = facet?.Name,
                    Filter = _searchFiltersBuilder.ToFilters(aggregation.Filter, schema),
                    Facet = facet,
                    FilterName = aggregation.Filter.ToString(),
                };

                results.Add(facetRequest);
            }
        }

        return results;
    }

    protected virtual IEnumerable<AggregationRequest> PreProcessAggregations(IEnumerable<AggregationRequest> aggregations)
    {
        return PrepareFacets(aggregations);
    }

    protected virtual Facet AddTermAggregationRequest(TermAggregationRequest termAggregationRequest, Schema schema)
    {
        if (string.IsNullOrEmpty(termAggregationRequest.FieldName))
        {
            return null;
        }

        var fieldName = _fieldNameConverter.ToProviderFieldName(termAggregationRequest.FieldName);
        var fieldType = schema.Fields.ContainsKey(fieldName) ? (FieldType?)schema.Fields[fieldName] : null;

        ValueFacet result = null;

        switch (fieldType)
        {
            case null:
                break;
            case FieldType.Text:
            case FieldType.Number:
            case FieldType.Date:
                result = new ValueFacet
                {
                    Name = fieldName,
                    Size = termAggregationRequest.Size == 0 || termAggregationRequest.Size > MaxFacetValues
                        ? MaxFacetValues
                        : termAggregationRequest.Size,
                };
                break;
            default:
                Debug.WriteLine("Elastic App Search supports value facet only for text, number and date fields.");
                break;
        }

        return result;
    }

    protected virtual Facet AddRangeAggregationRequest(RangeAggregationRequest rangeAggregationRequest, Schema schema)
    {
        if (string.IsNullOrEmpty(rangeAggregationRequest.FieldName))
        {
            return null;
        }

        var fieldName = _fieldNameConverter.ToProviderFieldName(rangeAggregationRequest.FieldName);
        var fieldType = schema.Fields.ContainsKey(fieldName) ? (FieldType?)schema.Fields[fieldName] : null;

        Facet result = null;

        switch (fieldType)
        {
            case null:
                break;
            case FieldType.Number:
                var ranges = rangeAggregationRequest.Values
                    .Select(range =>
                    {
                        var isFacetRangeValue = RangeFilterExtensions.TryParse(
                           range.IncludeLower, range.Lower,
                           range.IncludeUpper, range.Upper,
                           out FacetRangeValue<double> facetRangeValue);

                        var result = isFacetRangeValue ? facetRangeValue : null;

                        if (result != null)
                        {
                            result.Name = range.Id;
                        }

                        return result;
                    })
                    .Where(x => x != null)
                    .ToList();

                result = new NumberRangeFacet
                {
                    Ranges = ranges,
                };

                break;
            case FieldType.Date:
                result = new DateTimeRangeFacet
                {
                    Ranges = rangeAggregationRequest.Values.Select(x => new FacetRangeValue<DateTime>
                    {
                        From = ConvertToDateTime(x.Lower),
                        To = ConvertToDateTime(x.Upper),
                        Name = x.Id,
                    }).ToList()
                };

                break;
            case FieldType.Geolocation:
                result = new GeoLocationRangeFacet
                {
                    Ranges = rangeAggregationRequest.Values.Select(x => new FacetRangeValue<double>
                    {
                        From = ConvertToDouble(x.Lower),
                        To = ConvertToDouble(x.Upper),
                        Name = x.Id,
                    }).ToList()
                };

                break;
            default:
                Debug.WriteLine("Elastic App Search supports range facet only for date, number and geo location fields.");
                break;
        }

        if (result != null)
        {
            result.Name = fieldName;
        }

        return result;
    }

    private static DateTime? ConvertToDateTime(string input)
    {
        var result = (DateTime?)null;

        if (DateTime.TryParse(input, out var value))
        {
            result = value;
        }

        return result;
    }

    private static double? ConvertToDouble(string input)
    {
        var result = (double?)null;

        if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            result = value;
        }

        return result;
    }

    /// Try to fix faulty xapi logic
    private IEnumerable<AggregationRequest> PrepareFacets(IEnumerable<AggregationRequest> aggregations)
    {
        var result = new List<AggregationRequest>();

        foreach (var aggregation in aggregations ?? Array.Empty<AggregationRequest>())
        {
            var aggregationFilterFieldName = aggregation.FieldName ?? (aggregation.Filter as INamedFilter)?.FieldName;

            if (aggregation.Filter is AndFilter andFilter)
            {
                var clonedFilter = aggregation.Filter.Clone() as AndFilter;

                clonedFilter.ChildFilters = clonedFilter.ChildFilters
                    .Where(x =>
                    {
                        var result = true;

                        if (x is INamedFilter namedFilter)
                        {
                            result = !(aggregationFilterFieldName?.StartsWith(namedFilter.FieldName, true, CultureInfo.InvariantCulture) ?? false);
                        }

                        return result;
                    })
                    .ToList();

                if (clonedFilter.ChildFilters.Count == 1 && clonedFilter.ChildFilters[0] is AndFilter)
                {
                    aggregation.Filter = clonedFilter.ChildFilters[0];
                }
                else
                {
                    aggregation.Filter = clonedFilter;
                }
            }

            result.Add(aggregation);
        }

        return result;
    }
}
