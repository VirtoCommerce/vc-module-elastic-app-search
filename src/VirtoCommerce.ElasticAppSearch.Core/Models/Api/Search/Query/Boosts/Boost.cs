using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;

[JsonConverter(typeof(BoostConverter))]
public abstract class Boost
{
    [JsonRequired]
    public virtual string Type { get; set; }

    /// <summary>
    /// Must be between 0 and 10. Defaults to 1.0. A negative factor or fractional factor will not deboost a result.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public double? Factor { get; set; }
}
