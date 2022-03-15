using System.Globalization;
using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;
using SearchGeoPoint = VirtoCommerce.SearchModule.Core.Model.GeoPoint;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;

[JsonConverter(typeof(GeoPointConverter))]
public class GeoPoint: SearchGeoPoint
{
    public GeoPoint()
    {
    }

    public GeoPoint(SearchGeoPoint geoPoint)
    {
        Latitude = geoPoint.Latitude;
        Longitude = geoPoint.Longitude;
    }

    public override string ToString()
    {
        return $"{Latitude.ToString("#0.0######", CultureInfo.InvariantCulture)}, {Longitude.ToString("#0.0######", CultureInfo.InvariantCulture)}";
    }
}
