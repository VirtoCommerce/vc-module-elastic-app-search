namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;

public record Filters: IFilter
{
    public IFilter[] All { get; set; }

    public IFilter[] Any { get; set; }

    public IFilter[] None { get; set; }
}
