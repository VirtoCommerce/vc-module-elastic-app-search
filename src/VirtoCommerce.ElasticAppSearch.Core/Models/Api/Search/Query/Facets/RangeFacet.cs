using System.Collections.Generic;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets
{
    /// <summary>
    /// Available on number, date, geolocation fields.
    /// Not available for text.
    /// </summary>
    public class RangeFacet<TValue> : Facet where TValue : struct
    {
        public override string Type => "range";

        [JsonRequired]
        public IList<FacetRangeValue<TValue>> Ranges { get; set; } = new List<FacetRangeValue<TValue>>();
    }
}
