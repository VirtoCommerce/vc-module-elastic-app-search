using Newtonsoft.Json;
using static VirtoCommerce.ElasticAppSearch.Core.ModuleConstants.Api;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;

public class FunctionalBoost : Boost
{
    public override string Type => BoostTypes.Functional;

    [JsonRequired]
    public string Function { get; set; }

    public string Operation { get; set; }
}
