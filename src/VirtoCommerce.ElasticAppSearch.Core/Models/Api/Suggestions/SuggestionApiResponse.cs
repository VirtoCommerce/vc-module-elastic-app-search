using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Suggestions;

public class SuggestionApiResponse
{
    public Metadata Meta { get; init; }

    public SuggestionsApiResult Results { get; set; }
}

public class SuggestionsApiResult
{
    public SuggestionApiDocument[] Documents { get; set; }
}

public class SuggestionApiDocument
{
    public string Suggestion { get; set; }
}
