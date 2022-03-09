using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class FieldConverter<TField, TValue>: JsonConverter<TField>
    where TField: Field<TValue>, new()
{
    public override TField ReadJson(JsonReader reader, Type objectType, TField existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        if (token.Type != JTokenType.Null)
        {
            var jObject = (JObject)token;
            var property = jObject.Properties().Single();
            return new TField
            {
                FieldName = property.Name,
                Value = property.Value.ToObject<TValue>(serializer)
            };
        }
        return null;
    }

    public override void WriteJson(JsonWriter writer, TField value, JsonSerializer serializer)
    {
        if (value != null)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(value.FieldName);
            serializer.Serialize(writer, value.Value);
            writer.WriteEndObject();
        }
        else
        {
            writer.WriteNull();
        }
    }
}
