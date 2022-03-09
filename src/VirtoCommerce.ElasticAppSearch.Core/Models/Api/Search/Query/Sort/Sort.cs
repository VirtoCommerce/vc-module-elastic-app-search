using System.Collections.Generic;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sort;

[JsonArray(AllowNullItems = false)]
public class Sort: List<Field<SortOrder>>
{
    public Sort()
    {
    }

    public Sort(IEnumerable<Field<SortOrder>> enumerable) : base(enumerable)
    {
    }
}
