using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using SearchModuleCoreGeoPoint = VirtoCommerce.SearchModule.Core.Model.GeoPoint;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json
{
    public class SearchModuleCoreGeoPointConverter : JsonConverter<SearchModuleCoreGeoPoint>
    {
        public override SearchModuleCoreGeoPoint ReadJson(JsonReader reader, Type objectType, [AllowNull] SearchModuleCoreGeoPoint existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<string>(reader);
            var result = value != null ? SearchModuleCoreGeoPoint.Parse(value) : null;
            return result;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] SearchModuleCoreGeoPoint value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value?.ToString());
        }
    }
}
