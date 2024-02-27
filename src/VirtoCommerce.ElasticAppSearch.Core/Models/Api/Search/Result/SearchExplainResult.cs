using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record SearchExplainResult
{
    public Metadata Meta { get; init; }

    public string QueryString { get; init; }

    public JObject QueryBody { get; init; }
}
