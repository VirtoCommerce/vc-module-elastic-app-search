using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using static VirtoCommerce.ElasticAppSearch.Core.ModuleConstants.Api;

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
                    aggregation.Values = dataValue.Data.Select(d => GetAggregationResponseValue(d.Value, d.Count)).ToList();
                }

                return aggregation;
            });

            return result?.ToList();
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
            var facets = new Dictionary<string, FacetResult>();
            foreach (var item in facetResults.SelectMany(x => x).ToList())
            {
                if (!facets.ContainsKey(item.Key))
                {
                    facets.Add(item.Key, item.Value.FirstOrDefault());
                }
            }

            var statistics = new Dictionary<string, StatisticsResult[]>();
            if (aggregations != null)
            {
                foreach (var aggregation in aggregations.Where(x => x is RangeAggregationRequest))
                {
                    var statMin = GetStatResult(StatTypes.Min, searchResults, aggregation);
                    var statMax = GetStatResult(StatTypes.Max, searchResults, aggregation);

                    if (statMin != null && statMax != null)
                    {
                        statistics.TryAdd(aggregation.FieldName, [statMin, statMax]);
                    }
                }
            }

            return aggregations?
                .Select(x => GetAggregationResponseFromRequest(x, facets, statistics))
                .Where(x => x?.Values.Any() == true)
                ?? new List<AggregationResponse>();
        }

        private static StatisticsResult GetStatResult(string statType, IList<SearchResultAggregationWrapper> searchResults, AggregationRequest aggregation)
        {
            var statsIdMax = $"{aggregation.Id}-stats-{statType}";
            var data = searchResults.FirstOrDefault(x => x.AggregationId == statsIdMax)?.SearchResult?.Results?.FirstOrDefault();
            if (data != null)
            {
                return new StatisticsResult
                {
                    StatType = statType,
                    Data = data,
                };
            }

            return null;
        }

        /// <summary>
        /// Convert aggregations without field name (special case)
        /// </summary>
        private static IEnumerable<AggregationResponse> ToSimpleAggregationResponses(IList<SearchResultAggregationWrapper> searchResults)
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

        private AggregationResponse GetAggregationResponseFromRequest(AggregationRequest aggregationRequest, Dictionary<string, FacetResult> facets, Dictionary<string, StatisticsResult[]> statistics)
        {
            AggregationResponse result = null;

            switch (aggregationRequest)
            {
                case TermAggregationRequest termAggregationRequest:
                    result = GetAggregationResponseByTerm(termAggregationRequest, facets);
                    break;
                case RangeAggregationRequest rangeAggregationRequest:
                    result = GetAggregationResponseByRange(rangeAggregationRequest, facets, statistics);
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

        private AggregationResponse GetAggregationResponseByRange(RangeAggregationRequest rangeAggregationRequest, Dictionary<string, FacetResult> facets, Dictionary<string, StatisticsResult[]> statistics)
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

            // add statistics for range facets
            var statistic = statistics.GetValueOrDefault(fieldName);
            if (statistic != null)
            {
                result.Statistics = new AggregationStatistics
                {
                    Min = ParseStatistics(fieldName, StatTypes.Min, statistic),
                    Max = ParseStatistics(fieldName, StatTypes.Max, statistic)
                };
            }

            return result;
        }

        private static double? ParseStatistics(string fieldName, string statType, StatisticsResult[] statistic)
        {
            var fields = statistic.FirstOrDefault(x => x.StatType == statType)?.Data?.Fields;
            if (fields == null)
            {
                return null;
            }

            var value = fields.GetValueOrDefault(fieldName);
            if (value.Raw is JArray jArray)
            {
                var values = jArray.ToObject<object[]>();
                if (values != null && values.Length > 0)
                {
                    return Convert.ToDouble(values[0]);
                }
            }

            return null;
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
