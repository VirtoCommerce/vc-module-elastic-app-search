using System;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class CustomContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var jsonProperty = base.CreateProperty(member, memberSerialization);
        if (!jsonProperty.Ignored)
        {
            var emptyValueHandling = member.GetCustomAttribute<CustomJsonPropertyAttribute>()?.EmptyValueHandling;
            var ignoreEmpty = emptyValueHandling == EmptyValueHandling.Ignore;

            var shouldSerialize = jsonProperty.ShouldSerialize;
            jsonProperty.ShouldSerialize = obj => ShouldDo(jsonProperty, obj, ignoreEmpty, shouldSerialize);

            var shouldDeserialize = jsonProperty.ShouldDeserialize;
            jsonProperty.ShouldDeserialize = obj => ShouldDo(jsonProperty, obj, ignoreEmpty, shouldDeserialize);
        }
        return jsonProperty;
    }

    private static bool ShouldDo(JsonProperty jsonProperty, object obj, bool ignoreEmpty, Predicate<object> callback)
    {
        if (ignoreEmpty)
        {
            var value = jsonProperty.ValueProvider?.GetValue(obj);
            if (value is IEnumerable enumerable && !enumerable.GetEnumerator().MoveNext())
            {
                return false;
            }
        }

        return callback == null || callback(obj);
    }
}
