using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Engines;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record Metadata
{
    public string[] Alerts { get; init; }

    public string[] Warnings { get; init; }

    public int Precision { get; init; }

    public Page Page { get; init; }

    public Engine Engine { get; init; }
    
    public string RequestId { get; init; }
}
