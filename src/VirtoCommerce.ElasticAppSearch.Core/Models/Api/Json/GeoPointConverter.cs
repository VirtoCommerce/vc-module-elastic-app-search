using System;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class GeoPointConverter: JsonConverter<GeoPoint>
{
    public override GeoPoint ReadJson(JsonReader reader, Type objectType, GeoPoint existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var value = serializer.Deserialize<string>(reader);

        GeoPoint result = null;
        if (value != null)
        {
            result = new GeoPoint(SearchModule.Core.Model.GeoPoint.Parse(value));
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, GeoPoint value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value?.ToString()); 
    }
}
