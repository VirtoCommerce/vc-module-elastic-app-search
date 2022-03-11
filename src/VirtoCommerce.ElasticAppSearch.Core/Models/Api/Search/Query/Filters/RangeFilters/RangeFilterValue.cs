using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

public record RangeFilterValue<TValue>
    where TValue : struct
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TValue? From { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TValue? To { get; init; }
}
