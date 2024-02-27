using Newtonsoft.Json;
using static VirtoCommerce.ElasticAppSearch.Core.ModuleConstants.Api;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;

public class ProximityBoost : Boost
{
    public override string Type => BoostTypes.Proximity;

    [JsonRequired]
    public string Function { get; set; }

    [JsonRequired]
    public string Center { get; set; }
}
