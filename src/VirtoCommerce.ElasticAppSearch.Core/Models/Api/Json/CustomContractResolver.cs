using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VirtoCommerce.ElasticAppSearch.Core.Extensions;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class CustomContractResolver: DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var jsonProperty = base.CreateProperty(member, memberSerialization);
        if (!jsonProperty.Ignored)
        {
            var emptyValueHandling = member.GetCustomAttribute<CustomJsonPropertyAttribute>()?.EmptyValueHandling;
            var shouldIgnoreEmpty = emptyValueHandling == EmptyValueHandling.Ignore;

            var shouldSerialize = jsonProperty.ShouldSerialize;
            jsonProperty.ShouldSerialize = obj => ShouldDo(jsonProperty, obj, shouldIgnoreEmpty, shouldSerialize);

            var shouldDeserialize = jsonProperty.ShouldDeserialize;
            jsonProperty.ShouldDeserialize = obj => ShouldDo(jsonProperty, obj, shouldIgnoreEmpty, shouldDeserialize);
        }
        return jsonProperty;
    }

    private static bool ShouldDo(JsonProperty jsonProperty, object obj, bool shouldIgnoreEmpty, Predicate<object> callback)
    {
        var value = jsonProperty.ValueProvider?.GetValue(obj);
        var enumerable = value?.AsArray();
        var result = !shouldIgnoreEmpty || enumerable != null && enumerable.Any();
        if (callback != null)
        {
            result &= callback(obj);
        }

        return result;
    }
}
