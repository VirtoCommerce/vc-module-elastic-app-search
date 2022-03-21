using System.Collections.Generic;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record SearchResult
{
    public Metadata Meta { get; init; }

    public SearchResultDocument[] Results { get; init; }

    public Dictionary<string, FacetResult[]> Facets { get; init; }
}
