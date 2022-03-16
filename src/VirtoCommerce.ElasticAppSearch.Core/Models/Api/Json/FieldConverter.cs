using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class FieldConverter: JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsAssignableFrom(typeof(Field<>));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        if (token.Type != JTokenType.Null)
        {
            var jObject = (JObject)token;
            var property = jObject.Properties().Single();

            var result = Activator.CreateInstance(objectType);

            var fieldNameProperty = objectType.GetProperty(nameof(Field<object>.FieldName))!;
            fieldNameProperty.SetValue(result, property.Name);

            var valueProperty = objectType.GetProperty(nameof(Field<object>.Value))!;
            valueProperty.SetValue(result, property.Value.ToObject(valueProperty.GetType(), serializer));

            return result;
        }
        return null;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value != null)
        {
            var objectType = value.GetType();

            var fieldNameProperty = objectType.GetProperty(nameof(Field<object>.FieldName))!;
            var fieldName = (string)fieldNameProperty.GetValue(value)!;

            var valueProperty = objectType.GetProperty(nameof(Field<object>.Value))!;
            var fieldValue = valueProperty.GetValue(value)!;

            writer.WriteStartObject();
            writer.WritePropertyName(fieldName);
            serializer.Serialize(writer, fieldValue);
            writer.WriteEndObject();
        }
        else
        {
            writer.WriteNull();
        }
    }
}
