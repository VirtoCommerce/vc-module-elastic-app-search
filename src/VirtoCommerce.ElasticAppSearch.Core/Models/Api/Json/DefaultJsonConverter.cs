using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class DefaultJsonConverter: JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return JToken.Load(reader).ToObject(objectType, serializer);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JToken.FromObject(value, serializer).WriteTo(writer);
    }
}
