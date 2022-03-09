using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api;

public abstract record Field<TFieldValue>
{
    [JsonRequired]
    public virtual string FieldName { get; init; }

    [JsonRequired]
    public virtual TFieldValue Value { get; init; }
}
