using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;

[JsonObject(NamingStrategyType = typeof(DefaultNamingStrategy))]
public record Document
{
    [JsonProperty("id")]
    public virtual string Id { get; init; }

    [JsonExtensionData]
    public virtual Dictionary<string, object> Fields { get; } = new();
}
