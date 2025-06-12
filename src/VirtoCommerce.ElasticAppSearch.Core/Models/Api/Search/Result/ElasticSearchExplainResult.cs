using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record ElasticSearchExplainResult
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? Took { get; init; }

    public ElasticSearchExplainHits Hits { get; init; }
}

public record ElasticSearchExplainHits
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public double? MaxScore { get; init; }

    public ElasticSearchExplainHit[] Hits { get; init; }
}

public record ElasticSearchExplainHit
{
    [JsonProperty("_id")]
    public string Id { get; init; }

    [JsonProperty("_index")]
    public string Index { get; init; }

    [JsonProperty("_score", NullValueHandling = NullValueHandling.Ignore)]
    public double? Score { get; init; }

    [JsonProperty("_explanation")]
    public JObject Explanation { get; init; }
}
