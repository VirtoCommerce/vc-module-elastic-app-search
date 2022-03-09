using System.Collections.Generic;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.SearchFields;

public class SearchFields: Dictionary<string, SearchFieldValue>
{
    public SearchFields()
    {
    }

    public SearchFields(IDictionary<string, SearchFieldValue> dictionary) : base(dictionary)
    {
    }
}
