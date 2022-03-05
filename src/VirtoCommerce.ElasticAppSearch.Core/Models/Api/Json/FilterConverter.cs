using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

public class FilterConverter<TFiler, TFilterValue>: FieldConverter<TFiler, TFilterValue>
    where TFiler: Filter<TFilterValue>, new()
{
}
