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
                    aggregation.Values = dataValue.Data.Select(x => GetAggregationResponseValue(x.Value, x.Count)).ToList();
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

            switch (aggregationRequest)
            {
                case TermAggregationRequest termAggregationRequest:
                    result = GetAggregationResponseByTerm(termAggregationRequest, facets);
                    break;
                case RangeAggregationRequest rangeAggregationRequest:
                    result = GetAggregationResponseByRange(rangeAggregationRequest, facets);
                    break;
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
            };

            if (termAggregationRequest.Values == null)
            {
                // Return all found facet results is no values are defined
                result.Values = facet.Data
                    .Where(x => x.Count > 0)
                    .Select(x => GetAggregationResponseValue(x.Value, x.Count)).ToList();
            }
            else
            {
                result.Values = facet.Data
                    .Where(x => termAggregationRequest.Values.Any(v => ToStringInvariant(x.Value).EqualsInvariant(v)) && x.Count > 0)
                    .Select(x => GetAggregationResponseValue(x.Value, x.Count)).ToList();
            }

            return result;
        }

        private AggregationResponse GetAggregationResponseByRange(RangeAggregationRequest rangeAggregationRequest, Dictionary<string, FacetResult> facets)
        {
            if (string.IsNullOrEmpty(rangeAggregationRequest.FieldName))
            {
                return null;
            }

            var fieldName = _fieldNameConverter.ToProviderFieldName(rangeAggregationRequest.FieldName);
            var facet = facets.GetValueOrDefault(fieldName);

            if (facet == null)
            {
                return null;
            }

            var result = new AggregationResponse
            {
                Id = rangeAggregationRequest.Id ?? rangeAggregationRequest.FieldName,
            };

            if (rangeAggregationRequest.Values != null)
            {
                result.Values = facet.Data
                    .Where(x => rangeAggregationRequest.Values.Any(r => r.Id == x.Name) && x.Count > 0)
                    .Select(x => GetAggregationResponseValue(x.Name, x.Count)).ToList();
            }

            return result;
        }

        private static AggregationResponseValue GetAggregationResponseValue(string facetDataName, int? count)
        {
            return new AggregationResponseValue
            {
                Id = facetDataName,
                Count = count ?? 0,
            };
        }

        private static AggregationResponseValue GetAggregationResponseValue(object facetDataValue, int? count)
        {
            return new AggregationResponseValue
            {
                Id = ToStringInvariant(facetDataValue),
                Count = count ?? 0,
            };
        }

        private static string ToStringInvariant(object value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }
    }
}
