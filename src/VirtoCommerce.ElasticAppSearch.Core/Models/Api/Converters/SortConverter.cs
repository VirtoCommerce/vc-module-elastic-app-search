using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Converters
{
    /// <summary>
    /// Custom JSON converter for Sort property: the sort JSON object cannot have more than one key
    /// If more than two sort files are passed need to wrap them into array
    /// </summary>
    public class SortConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dictionary = value as Dictionary<string, string>;

            if (dictionary.Count == 1)
            {
                var item = dictionary.First();
                WriteObject(writer, item);
            }
            else if (dictionary.Count > 1)
            {
                writer.WriteStartArray();
                foreach (var item in dictionary)
                {
                    WriteObject(writer, item);
                }
                writer.WriteEndArray();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            var result = objectType == typeof(Dictionary<string, string>);
            return result;
        }

        private void WriteObject(JsonWriter writer, KeyValuePair<string, string> item)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(item.Key);
            writer.WriteValue(item.Value);
            writer.WriteEndObject();
        }
    }
}
