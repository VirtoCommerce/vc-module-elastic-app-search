using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;
using static VirtoCommerce.ElasticAppSearch.Core.ModuleConstants.Api;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class BoostConverter : JsonConverter<Dictionary<string, Boost[]>>
{
    public override Dictionary<string, Boost[]> ReadJson(JsonReader reader, Type objectType, Dictionary<string, Boost[]> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var boosts = new Dictionary<string, Boost[]>();

        var jsonObject = JObject.Load(reader);
        foreach (var (key, value) in jsonObject)
        {
            var boostArray = value.Children().Select<JToken, Boost>(token =>
            {
                var boostObject = (JObject)token;
                var type = boostObject.GetValue("type", StringComparison.OrdinalIgnoreCase)?.Value<string>();
                if (type == null)
                {
                    throw new JsonSerializationException("Boost type is required.");
                }

                return type switch
                {
                    BoostTypes.Value => boostObject.ToObject<ValueBoost>(serializer),
                    BoostTypes.Functional => boostObject.ToObject<FunctionalBoost>(serializer),
                    BoostTypes.Proximity => boostObject.ToObject<ProximityBoost>(serializer),
                    _ => throw new JsonSerializationException($"Unknown boost type: {type}"),
                };
            }).ToArray();

            boosts.Add(key, boostArray);
        }

        return boosts;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<string, Boost[]> value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        foreach (var (key, boostArray) in value)
        {
            writer.WritePropertyName(key);
            writer.WriteStartArray();
            foreach (var boost in boostArray)
            {
                serializer.Serialize(writer, boost);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }
}
