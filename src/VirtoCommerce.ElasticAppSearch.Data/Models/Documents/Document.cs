using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VirtoCommerce.ElasticAppSearch.Data.Models.Documents;

[JsonObject(NamingStrategyType = typeof(DefaultNamingStrategy))]
public record Document
{
    [JsonProperty("id")]
    public virtual string Id { get; set; }

    [JsonExtensionData]
    public virtual Dictionary<string, object> Fields { get; } = new();
}
