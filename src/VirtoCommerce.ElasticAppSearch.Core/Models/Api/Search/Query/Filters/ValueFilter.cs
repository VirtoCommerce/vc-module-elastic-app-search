using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;

public record ValueFilter<T> : Filter<T[]>
{
    [JsonConverter(typeof(ArrayConverter), SingleValueHandling.AsObject)]
    public sealed override T[] Value { get; init; }
}
