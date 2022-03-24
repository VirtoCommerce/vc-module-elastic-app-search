using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets
{
    public class GeoLocationRangeFacet : RangeFacet<double>
    {
        [JsonRequired]
        public GeoPoint Center { get; init; }

        [JsonRequired]
        public MeasurementUnit Unit { get; init; }
    }
}
