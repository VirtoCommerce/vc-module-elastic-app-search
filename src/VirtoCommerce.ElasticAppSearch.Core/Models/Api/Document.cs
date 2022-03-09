using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api;

[JsonObject(NamingStrategyType = typeof(DefaultNamingStrategy))]
public record Document<TFieldValue>
{
    [JsonRequired]
    [JsonProperty(ModuleConstants.Api.FieldNames.Id)]
    public virtual TFieldValue Id { get; init; }

    [JsonIgnore]
    public virtual Dictionary<string, TFieldValue> Fields { get; set; } = new();

    [JsonExtensionData]
    protected internal virtual Dictionary<string, JToken> RawFields { get; set; } = new();
};
