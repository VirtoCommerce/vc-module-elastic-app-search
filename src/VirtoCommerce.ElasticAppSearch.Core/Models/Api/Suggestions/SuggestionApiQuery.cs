using System.Collections.Generic;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Suggestions;

public class SuggestionApiQuery
{
    public string Query { get; set; }
    public SuggestionsApiQueryType Types { get; set; }
    public int Size { get; set; } = 5;
}

public class SuggestionsApiQueryType
{
    public SuggestionsApiQueryTypeDocument Documents { get; set; }
}

public class SuggestionsApiQueryTypeDocument
{
    public IList<string> Fields { get; set; }
}
