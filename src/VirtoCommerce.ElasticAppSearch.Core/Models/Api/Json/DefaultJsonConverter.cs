using System;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class DefaultJsonConverter<T>: JsonConverter<T>
{
    public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var result = hasExistingValue ? existingValue : (T)serializer.Deserialize(reader, objectType);
        return result;
    }

    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
