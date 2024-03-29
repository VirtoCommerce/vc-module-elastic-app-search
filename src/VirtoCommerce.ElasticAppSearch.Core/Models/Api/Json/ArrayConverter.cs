using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Extensions;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json
{
    public class ArrayConverter : JsonConverter
    {
        private readonly SingleValueHandling _singleValueHandling;
        private readonly JsonConverter _itemConverter;

        public ArrayConverter() : this(SingleValueHandling.AsArray)
        {
        }

        public ArrayConverter(SingleValueHandling singleValueHandling) : this(singleValueHandling, null)
        {
        }

        public ArrayConverter(SingleValueHandling singleValueHandling, Type itemConverterType) : this(singleValueHandling, itemConverterType, null)
        {
        }

        public ArrayConverter(SingleValueHandling singleValueHandling, Type itemConverterType, object[] itemConverterParameters)
        {
            _singleValueHandling = singleValueHandling;
            _itemConverter = itemConverterType != null
                ? (JsonConverter)Activator.CreateInstance(itemConverterType, itemConverterParameters)
                : new DefaultJsonConverter<object>();
        }

        public override bool CanConvert(Type objectType)
        {
            var result = objectType.IsAssignableFrom(typeof(IEnumerable)) && (_itemConverter?.CanConvert(objectType.GetEnumerableElementType()) ?? true);
            return result;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            if (token.Type != JTokenType.Null)
            {
                return Deserialize(token, objectType, existingValue, serializer);
            }

            return token.ToObject(objectType, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var array = value.AsArray();
                if (_singleValueHandling == SingleValueHandling.AsArray || array.Length > 1)
                {
                    Serialize(writer, array, serializer);
                }
                else
                {
                    var element = array.FirstOrDefault();
                    Serialize(writer, element, serializer);
                }
            }
            else
            {
                serializer.Serialize(writer, null);
            }
        }

        private object Deserialize(JToken token, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var elementType = objectType.GetEnumerableElementType();

            var list = new List<object>();
            if (token.Type == JTokenType.Array)
            {
                list.AddRange(((JArray)token).Select(elementToken => _itemConverter.ReadJson(elementToken.CreateReader(), elementType, null, serializer)));
            }
            else
            {
                list.Add(_itemConverter.ReadJson(token.CreateReader(), elementType, existingValue, serializer));
            }

            var result = serializer.Deserialize(JToken.FromObject(list, serializer).CreateReader(), objectType);
            return result;
        }

        private void Serialize(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Array array)
            {
                writer.WriteStartArray();
                foreach (var element in array)
                {
                    _itemConverter.WriteJson(writer, element, serializer);
                }
                writer.WriteEndArray();
            }
            else
            {
                _itemConverter.WriteJson(writer, value, serializer);
            }
        }
    }
}
