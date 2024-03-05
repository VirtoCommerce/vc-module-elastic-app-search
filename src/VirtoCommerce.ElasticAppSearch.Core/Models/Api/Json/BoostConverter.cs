using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;
using static VirtoCommerce.ElasticAppSearch.Core.ModuleConstants.Api;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class BoostConverter : CustomCreationConverter<Boost>
{
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var boostObject = JObject.ReadFrom(reader);
        var type = boostObject["type"].ToString();
        var actualObjectType = GetBoostType(type);
        var result = base.ReadJson(boostObject.CreateReader(), actualObjectType, existingValue, serializer);
        return result;
    }

    public override Boost Create(Type objectType)
    {
        return (Boost)Activator.CreateInstance(objectType);
    }

    private static Type GetBoostType(string type) => type switch
    {
        BoostTypes.Value => typeof(ValueBoost),
        BoostTypes.Functional => typeof(FunctionalBoost),
        BoostTypes.Proximity => typeof(ProximityBoost),
        _ => throw new JsonSerializationException($"Unknown boost type: {type}"),
    };
}
