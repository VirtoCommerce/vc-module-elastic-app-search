using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets
{
    /// <summary>
    /// Available on text, number, date fields.
    /// Not available for geolocation.
    /// </summary>
    public record ValueFacet : Facet
    {
        public override string Type => "value";

        /// <summary>
        /// Optional. How many facets you'd like to return. Can be between 1 and 250. 10 is the default.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Size { get; set; }

        /// <summary>
        /// Optional. Key is either 'count' or 'value' and the value is 'asc' or 'desc'.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FacetSort Sort { get; set; }
    }
}
