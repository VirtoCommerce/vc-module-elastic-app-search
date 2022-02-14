using System.Collections.Generic;
using System.Linq;

namespace VirtoCommerce.ElasticAppSearch.Data.Models;

public class Schema: Dictionary<string, FieldType>
{
    public Schema()
    {
    }

    public Schema(IEnumerable<Schema> schemas)
        : base(schemas
            .SelectMany(schema => schema)
            .ToLookup(x => x.Key, x => x.Value)
            .ToDictionary(x => x.Key, x => x.First()))
    {
    }
}
