using System.Collections.Generic;
using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Converters;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

public record SearchQuery
{
    public string Query { get; set; }

    [JsonConverter(typeof(SortConverter))]
    public IList<SearchQuerySortField> Sort { get; set; }

    // Conditional property for sort: Sort cannot be empty or null
    public bool ShouldSerializeSort()
    {
        return Sort != null && Sort.Count > 0;
    }

    public SearchQueryPage Page { get; set; }

    public Dictionary<string, object> SearchFields { get; set; }
}
