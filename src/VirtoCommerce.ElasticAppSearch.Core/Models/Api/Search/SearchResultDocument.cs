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
        var rawId = RawFields["id"]["raw"];
        RawFields.Remove("id");
        Id = rawId.Value<string>();

        foreach (var (fieldName, value) in RawFields)
        {
            var raw = value["raw"];

            var rawValue = (raw as JValue)?.Value;
            var rawObject = (raw as JObject)?.ToObject<object>();
            var rawArray = (raw as JArray)?.ToObject<object[]>();

            var fieldValue = rawValue ?? rawObject ?? rawArray;

            Fields.Add(fieldName, fieldValue);
        }
    }
}
