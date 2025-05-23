using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Synonyms;

public record SynonymApiQuery
{
    public SynonymApiQuery()
    {
    }

    public SynonymApiQuery(int page, int size)
    {
        Page = new Page
        {
            Current = page,
            Size = size,
        };
    }

    public Page Page { get; set; }
}
