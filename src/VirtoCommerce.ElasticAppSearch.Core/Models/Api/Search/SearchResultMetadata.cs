using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Engines;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

public record SearchResultMetadata
{
    public string[] Alerts { get; set; }

    public string[] Warnings { get; set; }

    public int Precision { get; set; }

    public SearchResultPage Page { get; set; }

    public Engine Engine { get; set; }
    
    public string RequestId { get; set; }
}
