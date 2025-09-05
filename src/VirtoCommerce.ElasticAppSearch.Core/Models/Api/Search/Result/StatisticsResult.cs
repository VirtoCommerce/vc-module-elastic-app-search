namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record StatisticsResult
{
    public string StatType { get; init; }

    public SearchResultDocument Data { get; init; }
}
