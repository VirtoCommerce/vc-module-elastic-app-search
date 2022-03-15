using System.Collections.Generic;
using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;

public record SearchQuery
{
    [JsonRequired]
    public string Query { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [CustomJsonProperty(EmptyValueHandling = EmptyValueHandling.Ignore)]
    [JsonConverter(typeof(ArrayConverter), SingleValueHandling.AsObject)]
    public Field<SortOrder>[] Sort { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IFilters Filters { get; init; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [CustomJsonProperty(EmptyValueHandling = EmptyValueHandling.Ignore)]
    public Dictionary<string, SearchFieldValue> SearchFields { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Page Page { get; init; }
}
