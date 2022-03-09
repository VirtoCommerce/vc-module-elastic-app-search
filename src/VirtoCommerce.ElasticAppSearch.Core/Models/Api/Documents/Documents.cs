using System.Collections.Generic;
using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;

[JsonArray(ItemConverterType = typeof(DocumentConverter<Document, object>))]
public class Documents: List<Document>
{
    public Documents()
    {
    }

    public Documents(IEnumerable<Document> collection): base(collection)
    {
    }
}
