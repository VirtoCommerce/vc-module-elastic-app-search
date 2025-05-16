using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Synonyms;

public record SynonymApiResponse
{
    public SynonymApiMetadata Meta { get; init; }

    public SynonymApiDocument[] Results { get; set; }
}

public record SynonymApiMetadata
{
    public Page Page { get; set; }
}

public record SynonymApiDocument : SynonymSet
{
    public string Id { get; set; }
}
