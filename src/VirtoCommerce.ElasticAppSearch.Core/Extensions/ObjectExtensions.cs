using System.Collections;
using System.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using SearchGeoPoint = VirtoCommerce.SearchModule.Core.Model.GeoPoint;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class ObjectExtensions
{
    public static object[] AsArray(this object value)
    {
        return value as object[] ?? (value as IEnumerable)?.Cast<object>().ToArray();
    }

    public static GeoPoint ToGeoPoint(this SearchGeoPoint point)
    {
        return point == null ? null : new GeoPoint(point);
    }
}
