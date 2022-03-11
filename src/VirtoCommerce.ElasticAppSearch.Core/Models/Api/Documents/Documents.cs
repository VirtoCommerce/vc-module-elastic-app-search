using System.Collections.Generic;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;

[JsonArray(AllowNullItems = false)]
public class Documents: List<Document>
{
    public Documents()
    {
    }

    public Documents(IEnumerable<Document> collection): base(collection)
    {
    }
}
