using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets;

public class Facet
{
    [JsonRequired]
    public virtual string Type { get; }

    /// <summary>
    /// Optional. Name given to facet.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }
}
