using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sort;

[JsonConverter(typeof(FieldConverter))]
public record SortField : Field<SortOrder>
{
}
