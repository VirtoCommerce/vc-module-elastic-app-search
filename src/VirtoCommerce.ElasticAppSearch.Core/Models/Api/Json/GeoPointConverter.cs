using System;
using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class GeoPointConverter: JsonConverter<GeoPoint>
{
    public override GeoPoint ReadJson(JsonReader reader, Type objectType, GeoPoint existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var value = serializer.Deserialize<string>(reader);
        var result = value != null ? new GeoPoint(SearchModule.Core.Model.GeoPoint.Parse(value)) : null;
        return result;
    }

    public override void WriteJson(JsonWriter writer, GeoPoint value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        writer.WriteRaw(value?.ToString());
        writer.WriteEndArray();
    }
}
