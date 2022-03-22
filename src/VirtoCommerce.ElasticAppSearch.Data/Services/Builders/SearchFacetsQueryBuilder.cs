using System.Collections.Generic;
using System.Diagnostics;
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

    public IList<FacetRequest> GetFacetRequests(IList<AggregationRequest> aggregations, Schema schema)
    {
        var results = new List<FacetRequest>();

        if (aggregations.IsNullOrEmpty())
        {
            return results;
        }

        foreach (var aggregation in aggregations)
        {
            if (aggregation is TermAggregationRequest termAggregationRequest)
            {
                var facet = AddTermAggregationRequest(termAggregationRequest, schema);

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

        ValueFacet result;

        switch (fieldType)
        {
            case null:
                result = null;
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
                result = null;
                break;
        }

        return result;
    }
}
