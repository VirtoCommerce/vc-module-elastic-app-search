using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;

public record ResultFieldRaw
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? Size { get; init; }
}
