using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Data.Models;

public record Schema
{
    public void Merge(IEnumerable<Schema> schemas)
    {
        Fields = schemas.SelectMany(schema => schema.Fields).ToLookup(x => x.Key, x => x.Value).ToDictionary(x => x.Key, x => x.First());
    }

    [JsonExtensionData]
    public Dictionary<string, FieldType> Fields { get; set; }
}
