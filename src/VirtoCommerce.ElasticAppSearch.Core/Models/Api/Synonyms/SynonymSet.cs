using System.Collections.Generic;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Synonyms;

public record SynonymSet
{
    public SynonymSet()
    {
    }

    public SynonymSet(IList<string> synonyms)
    {
        Synonyms = synonyms ?? [];
    }

    public IList<string> Synonyms { get; set; }
}
