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

        public IList<AggregationResponse> ToAggregationResult(SearchResult searchResult)
        {
            var result = searchResult.Facets?.Select(x =>
            {
                var aggregation = new AggregationResponse
                {
                    Id = x.Key,
                };

                var dataValue = x.Value.FirstOrDefault();
                if (dataValue?.Data.Any() == true)
                {
                    aggregation.Values = dataValue.Data.Select(x => new AggregationResponseValue
                    {
                        Id = ToStringInvariant(x.Value),
                        Count = x.Count ?? 0
                    }).ToList();
                }

                return aggregation;
            });

            return result.ToList();
        }

        public IList<AggregationResponse> ToAggregationResult(IList<SearchResultAggregationWrapper> searchResults, IList<AggregationRequest> aggregations)
        {
            var result = new List<AggregationResponse>();

            var responses = ToFacetsAggregationResponses(searchResults, aggregations);
            result.AddRange(responses);

            responses = ToSimpleAggregationResponses(searchResults);
            result.AddRange(responses);

            return result;
        }

        /// <summary>
        /// Convert facets results
        /// </summary>
        private IEnumerable<AggregationResponse> ToFacetsAggregationResponses(IList<SearchResultAggregationWrapper> searchResults, IList<AggregationRequest> aggregations)
        {
            var facetResults = searchResults.Where(x => !x.SearchResult.Facets.IsNullOrEmpty()).Select(x => x.SearchResult.Facets).ToList();

            // combine all result facets in one dictionary
            var facets = facetResults.SelectMany(x => x).ToDictionary(x => x.Key, x => x.Value.FirstOrDefault());

            return aggregations?
                .Select(x => GetAggregationResponseFromRequest(x, facets))
                .Where(x => x?.Values.Any() == true)
                ?? new List<AggregationResponse>();
        }

        /// <summary>
        /// Convert aggregations without field name (special case)
        /// </summary>
        private IEnumerable<AggregationResponse> ToSimpleAggregationResponses(IList<SearchResultAggregationWrapper> searchResults)
        {
            return searchResults
                .Where(x => !string.IsNullOrEmpty(x.AggregationId) && x.SearchResult.Meta.Page.TotalResults > 0)
                .Select(x => new AggregationResponse
                {
                    Id = x.AggregationId,
                    Values = new List<AggregationResponseValue>
                            {
                                new AggregationResponseValue
                                {
                                    Id = x.AggregationId,
                                    Count = x.SearchResult.Meta.Page.TotalResults,
                                }
                            }
                });
        }

        private AggregationResponse GetAggregationResponseFromRequest(AggregationRequest aggregationRequest, Dictionary<string, FacetResult> facets)
        {
            AggregationResponse result = null;

            if (aggregationRequest is TermAggregationRequest termAggregationRequest)
            {
                result = GetAggregationResponseByTerm(termAggregationRequest, facets);
            }

            return result;
        }

        private AggregationResponse GetAggregationResponseByTerm(TermAggregationRequest termAggregationRequest, Dictionary<string, FacetResult> facets)
        {
            if (string.IsNullOrEmpty(termAggregationRequest.FieldName))
            {
                return null;
            }

            var fieldName = _fieldNameConverter.ToProviderFieldName(termAggregationRequest.FieldName);
            var facet = facets.GetValueOrDefault(fieldName);

            if (facet == null)
            {
                return null;
            }

            var result = new AggregationResponse
            {
                Id = termAggregationRequest.Id ?? termAggregationRequest.FieldName,
                Values = new List<AggregationResponseValue>(),
            };

            if (termAggregationRequest.Values == null)
            {
                // Return all found facet results is no values is defined
                foreach (var facetResult in facet.Data)
                {
                    var aggregationValue = GetAggregationResponseValue(ToStringInvariant(facetResult.Value), facetResult.Count);
                    result.Values.Add(aggregationValue);
                }
            }
            else
            {
                foreach (var value in termAggregationRequest.Values)
                {
                    var facetResult = facet.Data.FirstOrDefault(r => ToStringInvariant(r.Value).EqualsInvariant(value));

                    if (facetResult?.Count > 0)
                    {
                        var aggregationValue = GetAggregationResponseValue(value, facetResult.Count);
                        result.Values.Add(aggregationValue);
                    }
                }
            }

            return result;
        }

        private static AggregationResponseValue GetAggregationResponseValue(string value, int? count)
        {
            return new AggregationResponseValue
            {
                Id = value,
                Count = count ?? 0,
            };
        }

        private static string ToStringInvariant(object value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }
    }
}
