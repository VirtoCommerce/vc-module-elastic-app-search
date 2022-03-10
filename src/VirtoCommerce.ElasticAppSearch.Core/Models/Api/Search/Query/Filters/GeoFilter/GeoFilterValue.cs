using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.GeoFilter;

public record GeoFilterValue: RangeFilterValue<double>
{
    [JsonRequired]
    public GeoPoint Center { get; init; }

    [JsonRequired]
    public MeasurementUnit Unit { get; init; }
    
    public double Distance { get; init; }
};
