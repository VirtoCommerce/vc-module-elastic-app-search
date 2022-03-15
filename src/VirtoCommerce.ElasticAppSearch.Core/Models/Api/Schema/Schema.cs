using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;

[JsonConverter(typeof(DocumentConverter<Schema, FieldType>))]
public record Schema: DocumentBase<FieldType>
{
    [JsonIgnore]
    public override FieldType Id => FieldType.Text;

    public void Merge(IEnumerable<Schema> schemas)
    {
        Fields = schemas.SelectMany(schema => schema.Fields).ToLookup(x => x.Key, x => x.Value).ToDictionary(x => x.Key, x => x.First());
    }
}
