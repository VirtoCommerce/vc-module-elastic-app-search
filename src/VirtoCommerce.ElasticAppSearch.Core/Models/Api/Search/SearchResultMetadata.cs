using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Engines;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

public record SearchResultMetadata
{
    public string[] Alerts { get; init; }

    public string[] Warnings { get; init; }

    public int Precision { get; init; }

    public SearchResultPage Page { get; init; }

    public Engine Engine { get; init; }
    
    public string RequestId { get; init; }
}
