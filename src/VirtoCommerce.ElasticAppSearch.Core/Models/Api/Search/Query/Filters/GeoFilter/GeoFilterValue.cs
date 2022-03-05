using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.GeoFilter;

public record GeoFilterValue: RangeFilterValue<double>
{
    public GeoPoint Center { get; init; }

    public MeasurementUnit Unit { get; init; }

    public double Distance { get; init; }
};
