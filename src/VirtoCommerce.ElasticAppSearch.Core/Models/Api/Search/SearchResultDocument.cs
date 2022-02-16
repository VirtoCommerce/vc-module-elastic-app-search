using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

public record SearchResultDocument: Document
{
    [JsonIgnore]
    public override string Id { get; set; }

    [JsonIgnore]
    public override Dictionary<string, object> Fields { get; } = new();

    [JsonProperty("_meta")]
    public virtual SearchResultDocumentMetadata Meta { get; set; }

    [JsonExtensionData]
    protected virtual Dictionary<string, JToken> RawFields { get; } = new();

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        foreach (var (fieldName, value) in RawFields)
        {
            var raw = value["raw"];
            var rawValue = raw as JValue;
            var rawObject = raw as JObject;
            var rawArray = raw as JArray;
            var fieldValue = rawValue?.Value ?? rawObject?.ToObject<object>() ?? rawArray?.ToObject<object[]>();
            Fields.Add(fieldName, fieldValue);
        }
    }
}
