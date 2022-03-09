using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record SearchResult
{
    public Metadata Meta { get; init; }

    [JsonProperty(ItemConverterType = typeof(DocumentConverter<Document, FieldValue>))]
    public Document[] Results { get; init; }
}
