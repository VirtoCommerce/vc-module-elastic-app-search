using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Data.Services.Builders
{
    public class AggregationsResponseBuilder : IAggregationsResponseBuilder
    {
        private readonly IFieldNameConverter _fieldNameConverter;

        public AggregationsResponseBuilder(IFieldNameConverter fieldNameConverter)
        {
            _fieldNameConverter = fieldNameConverter;
        }

        public IList<AggregationResponse> ToAggregationResult(IList<SearchResultAggregationWrapper> searchResults, IList<AggregationRequest> aggregations)
        {
            var result = new List<AggregationResponse>();

            // convert facets results
            var facets = new Dictionary<string, IList<FacetResult>>();
            var facetResults = searchResults.Where(x => !x.SearchResult.Facets.IsNullOrEmpty()).Select(x => x.SearchResult.Facets).ToList();
            foreach (var facetResul in facetResults)
            {
                foreach (var facet in facetResul)
                {
                    facets.Add(facet.Key, facet.Value);
                }
            }

            if (facets.Any())
            {
                var responses = aggregations
                    .Select(a => GetAggregation(a, facets))
                    .Where(a => a != null && a.Values.Any());

                result.AddRange(responses);
            }

            // aggregations with empty field name
            foreach (var searchResultWrapper in searchResults.Where(x => !string.IsNullOrEmpty(x.AggregationId) && x.SearchResult.Meta.Page.TotalResults > 0))
            {
                result.Add(new AggregationResponse
                {
                    Id = searchResultWrapper.AggregationId,
                    Values = new List<AggregationResponseValue>
                        {
                            new AggregationResponseValue
                            {
                                Id= searchResultWrapper.AggregationId,
                                Count = searchResultWrapper.SearchResult.Meta.Page.TotalResults,
                            }
                        }
                });
            }

            return result;
        }

        private AggregationResponse GetAggregation(AggregationRequest aggregationRequest, Dictionary<string, IList<FacetResult>> facets)
        {
            AggregationResponse result = null;

            if (!(aggregationRequest is TermAggregationRequest termAggregationRequest) || string.IsNullOrEmpty(termAggregationRequest.FieldName))
            {
                return result;
            }

            var fieldName = _fieldNameConverter.ToProviderFieldName(termAggregationRequest.FieldName);

            if (!string.IsNullOrEmpty(fieldName))
            {
                var facetResults = facets.ContainsKey(fieldName) ? facets[fieldName] : null;

                var facet = facetResults?.FirstOrDefault();
                if (facet != null && !facet.Data.IsNullOrEmpty())
                {
                    result = new AggregationResponse
                    {
                        Id = (termAggregationRequest.Id ?? termAggregationRequest.FieldName).ToLowerInvariant(),
                        Values = new List<AggregationResponseValue>(),
                    };

                    var values = termAggregationRequest.Values;

                    if (values != null)
                    {
                        foreach (var value in values)
                        {
                            var facetResult = facet.Data.FirstOrDefault(r => ToStringInvariant(r.Value).EqualsInvariant(value));

                            if (facetResult != null && facetResult.Count > 0)
                            {
                                var aggregationValue = new AggregationResponseValue
                                {
                                    Id = value,
                                    Count = facetResult.Count ?? 0,
                                };
                                result.Values.Add(aggregationValue);
                            }
                        }
                    }
                    else
                    {
                        // Return all facet results if values are not defined
                        foreach (var facetResult in facet.Data)
                        {
                            var aggregationValue = new AggregationResponseValue
                            {
                                Id = ToStringInvariant(facetResult.Value),
                                Count = facetResult.Count ?? 0,
                            };
                            result.Values.Add(aggregationValue);
                        }
                    }
                }
            }

            return result;
        }

        public string ToStringInvariant(object value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }
    }
}
