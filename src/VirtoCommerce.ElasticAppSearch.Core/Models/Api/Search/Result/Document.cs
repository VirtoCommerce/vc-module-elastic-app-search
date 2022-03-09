using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

public record Document: Document<FieldValue>
{
    [JsonProperty("_meta")]
    public virtual DocumentMetadata Meta { get; init; }
}
