using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;

public record Schema
{
    [JsonExtensionData]
    private Dictionary<string, object> Content => Fields.ToDictionary(x => x.Key, x => (object)x.Value);

    [JsonIgnore]
    public Dictionary<string, FieldType> Fields { get; private set; } = new();

    public void Merge(IEnumerable<Schema> schemas)
    {
        Fields = schemas.SelectMany(schema => schema.Fields).ToLookup(x => x.Key, x => x.Value).ToDictionary(x => x.Key, x => x.First());
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        Fields = Content.ToDictionary(x => x.Key, x => Enum.Parse<FieldType>((string)x.Value));
    }
}
