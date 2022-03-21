using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets
{
    public record FacetRangeValue<TValue> : RangeFilterValue<TValue> where TValue : struct
    {
        /// <summary>
        /// Optional. Name of the range.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }
}
