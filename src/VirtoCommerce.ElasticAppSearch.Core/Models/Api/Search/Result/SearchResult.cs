using System.Collections.Generic;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record SearchResult
{
    public Metadata Meta { get; init; }

    public SearchResultDocument[] Results { get; init; }

    public Dictionary<string, FacetResult[]> Facets { get; init; }
}

public record FacetResult
{
    public string Type { get; init; }

    public string Name { get; init; }

    public FacetData[] Data { get; init; }
}

public record FacetData
{
    public string Name { get; init; }

    public object Value { get; init; }

    public object From { get; init; }

    public object To { get; init; }

    public int? Count { get; init; }
}
