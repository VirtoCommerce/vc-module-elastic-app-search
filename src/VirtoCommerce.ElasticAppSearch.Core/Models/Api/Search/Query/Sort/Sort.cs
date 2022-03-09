using System.Collections.Generic;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sort;

[JsonArray(AllowNullItems = false)]
public class Sort: List<SortField>
{
    public Sort()
    {
    }

    public Sort(IEnumerable<SortField> enumerable) : base(enumerable)
    {
    }
}
