namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api;

public record DocumentResult
{
    public string Id { get; set; }

    public string[] Errors { get; set; }
}
