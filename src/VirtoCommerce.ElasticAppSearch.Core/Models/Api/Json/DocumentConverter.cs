using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class DocumentConverter<TDocument, TFieldValue> : DefaultJsonConverter<TDocument>
    where TDocument : DocumentBase<TFieldValue>
{
    public override TDocument ReadJson(JsonReader reader, Type objectType, TDocument existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var document = base.ReadJson(reader, objectType, existingValue, hasExistingValue, serializer);
        document.Fields = document.RawFields.ToDictionary(
            rawField => rawField.Key,
            rawField => rawField.Value.ToObject<TFieldValue>(serializer)
        );
        return document;
    }

    public override void WriteJson(JsonWriter writer, TDocument value, JsonSerializer serializer)
    {
        value.RawFields = value.Fields
            .Where(field => field.Value is not null)
            .ToDictionary(field => field.Key, field => JToken.FromObject(field.Value, serializer));
        base.WriteJson(writer, value, serializer);
    }
}
