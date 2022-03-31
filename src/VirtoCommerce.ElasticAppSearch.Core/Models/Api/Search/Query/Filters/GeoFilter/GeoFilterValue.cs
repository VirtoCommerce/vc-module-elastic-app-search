using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.GeoFilter;

public record GeoFilterValue: RangeValue<double>
{
    [JsonRequired]
    public GeoPoint Center { get; init; }

    [JsonRequired]
    public MeasurementUnit Unit { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public double? Distance { get; init; }
};
