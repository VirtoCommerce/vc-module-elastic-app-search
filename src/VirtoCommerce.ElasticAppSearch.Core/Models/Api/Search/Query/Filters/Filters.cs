using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;

public record Filters : IFilter
{
    [JsonProperty(ItemConverterType = typeof(FilterConverter))]
    public IFilter[] All { get; set; }

    [JsonProperty(ItemConverterType = typeof(FilterConverter))]
    public IFilter[] Any { get; set; }

    [JsonProperty(ItemConverterType = typeof(FilterConverter))]
    public IFilter[] None { get; set; }
}
