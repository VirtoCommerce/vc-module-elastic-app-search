using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

public record SearchSettings
{
    public JObject SearchFields { get; init; }

    public JObject ResultFields { get; init; }

    [JsonConverter(typeof(BoostConverter))]
    public Dictionary<string, Boost[]> Boosts { get; init; } = new();

    public int Precision { get; init; }

    public bool PrecisionEnabled { get; init; }

}
