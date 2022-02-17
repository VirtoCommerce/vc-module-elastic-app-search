using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Converters
{
    /// <summary>
    /// Custom JSON converter for Sort property: the sort JSON object cannot have more than one key
    /// If more than two sort fields are passed need to wrap them into array
    /// </summary>
    public class SortConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = value as IList<SearchQuerySortField>;

            if (list.Count == 1)
            {
                var item = list.First();
                WriteObject(writer, item);
            }
            else if (list.Count > 1)
            {
                writer.WriteStartArray();
                foreach (var item in list)
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
            var result = objectType == typeof(IList<SearchQuerySortField>);
            return result;
        }

        private void WriteObject(JsonWriter writer, SearchQuerySortField item)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(item.Field);
            writer.WriteValue(item.Order);
            writer.WriteEndObject();
        }
    }
}
