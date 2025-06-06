using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record ElasticSearchExplainResult
{
    public int Took { get; init; }

    public ElasticSearchExplainHits Hits { get; init; }
}

public record ElasticSearchExplainHits
{
    public double? MaxScore { get; init; }

    public ElasticSearchExplainHit[] Hits { get; init; }
}

public record ElasticSearchExplainHit
{
    public string Id { get; init; }

    public string Index { get; init; }

    public double? Score { get; init; }

    public JObject Explanation { get; init; }
}
