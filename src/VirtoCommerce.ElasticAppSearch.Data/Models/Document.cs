using System.Collections.Generic;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Data.Models;

public class Document
{
    [JsonProperty("id")]
    public virtual string Id { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> Content { get; } = new();
}
