using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.SearchFields;

public record SearchFieldValue
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? Weight { get; init; }
};
