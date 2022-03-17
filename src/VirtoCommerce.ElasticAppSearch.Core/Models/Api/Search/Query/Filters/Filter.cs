using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;

[JsonConverter(typeof(FilterConverter))]
public record Filter<T> : Field<T>, IFilter
{
}
