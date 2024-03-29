using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;
using static VirtoCommerce.ElasticAppSearch.Core.ModuleConstants.Api;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;

/// <summary>
/// Available on text, number, and date fields.
/// </summary>
public class ValueBoost : Boost
{
    public override string Type => BoostTypes.Value;

    [JsonRequired]
    [JsonConverter(typeof(ArrayConverter), SingleValueHandling.AsObject)]
    public string[] Value { get; set; }

    /// <summary>
    /// Can be "add" or "multiply". Defaults to "add".
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Operation { get; set; }
}
