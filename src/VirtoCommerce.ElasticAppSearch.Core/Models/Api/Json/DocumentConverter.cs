using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class DocumentConverter<TFieldValue>: JsonConverter<Document<TFieldValue>>
{
    public override Document<TFieldValue> ReadJson(JsonReader reader, Type objectType, Document<TFieldValue> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var document = hasExistingValue ? existingValue : JToken.Load(reader).ToObject(objectType, serializer) as Document<TFieldValue>;
        
        document.Fields = document.RawFields.ToDictionary(
            rawField => rawField.Key,
            rawField => rawField.Value.ToObject<TFieldValue>(serializer)
        );

        return document;
    }
    
    public override void WriteJson(JsonWriter writer, Document<TFieldValue> value, JsonSerializer serializer)
    {
        value.RawFields = value.Fields.ToDictionary(field => field.Key, field => JToken.FromObject(field.Value, serializer));
        var token = JToken.FromObject(value, serializer);
        token.WriteTo(writer);
    }
}
