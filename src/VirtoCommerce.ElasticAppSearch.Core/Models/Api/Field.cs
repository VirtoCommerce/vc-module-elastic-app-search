using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api;

[JsonConverter(typeof(FieldConverter))]
public record Field<TFieldValue>
{
    [JsonRequired]
    public virtual string FieldName { get; init; }

    [JsonRequired]
    public virtual TFieldValue Value { get; init; }
}
