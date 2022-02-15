using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VirtoCommerce.ElasticAppSearch.Data.Models.Search;

public record SearchResult
{
    public SearchResultMetadata Meta { get; set; }

    [JsonProperty(NamingStrategyType = typeof(DefaultNamingStrategy))]
    public SearchResultDocument[] Results { get; set; }
}
