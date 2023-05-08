using System.Collections.Generic;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Suggestions;

public class SuggestionApiQuery
{
    [JsonRequired]
    public string Query { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
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
