using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
                result = new NumberRangeFacet
                {
                    Ranges = rangeAggregationRequest.Values.Select(x => new FacetRangeValue<double>
                    {
                        From = ConvertToDouble(x.Lower),
                        To = ConvertToDouble(x.Upper),
                        Name = x.Id,
                    }).ToList()
                };

                break;
            case FieldType.Date:
                result = new DateTimeRangeFacet
                {
                    Ranges = rangeAggregationRequest.Values.Select(x => new FacetRangeValue<DateTime>
                    {
                        //From = ConvertToDouble(x.Lower),
                        //To = ConvertToDouble(x.Upper),
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

    private static double? ConvertToDouble(string input)
    {
        var result = (double?)null;

        if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            result = value;
        }

        return result;
    }
}
