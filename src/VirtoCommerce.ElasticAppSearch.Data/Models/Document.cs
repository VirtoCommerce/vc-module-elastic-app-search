using System.Collections.Generic;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Data.Models;

public record Document
{
    public virtual string Id { get; set; }

    [JsonExtensionData]
    public virtual Dictionary<string, object> Content { get; } = new();
}
