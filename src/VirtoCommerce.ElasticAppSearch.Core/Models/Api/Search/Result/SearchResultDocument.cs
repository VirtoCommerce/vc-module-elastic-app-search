using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

[JsonConverter(typeof(DocumentConverter<SearchResultDocument, FieldValue>))]
public record SearchResultDocument: DocumentBase<FieldValue>
{
    [JsonProperty("_meta")]
    public virtual DocumentMetadata Meta { get; init; }
}
