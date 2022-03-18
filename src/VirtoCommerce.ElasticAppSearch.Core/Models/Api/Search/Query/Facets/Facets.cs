using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets
{
    public class Facets : Dictionary<string, Facet>
    {
        public Facets()
        {
        }

        public Facets(IDictionary<string, Facet> dictionary) : base(dictionary)
        {
        }
    }

    public record class Facet
    {
        [JsonRequired]
        public virtual string Type { get; }

        /// <summary>
        /// Optional. Name given to facet.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }

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

    public enum FacetSortField
    {
        Count,
        Value
    }

    public record FacetSort : Field<SortOrder>
    {
        public FacetSort(FacetSortField field, SortOrder sortOrder)
        {
            FieldName = field.ToString().ToLowerInvariant();
            Value = sortOrder;
        }
    }

    /// <summary>
    /// Available on number, date, geolocation fields.
    /// Not availavle for text.
    /// </summary>
    public record RangeFacet<TValue> : Facet where TValue : struct
    {
        public override string Type => "range";

        [JsonRequired]
        public IList<FacetRangeValue<TValue>> Ranges { get; set; } = new List<FacetRangeValue<TValue>>();
    }

    public record DateTimeRangeFacet : RangeFacet<DateTime>
    {
    }

    public record NumberRangeFacet : RangeFacet<double>
    {
    }

    public record GeoLocationRangeFacet : RangeFacet<double>
    {
        [JsonRequired]
        public GeoPoint Center { get; init; }

        [JsonRequired]
        public MeasurementUnit Unit { get; init; }
    }

    public record FacetRangeValue<TValue> : RangeFilterValue<TValue> where TValue : struct
    {
        /// <summary>
        /// Optional. Name of the range.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }
}
