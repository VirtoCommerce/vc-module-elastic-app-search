using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

public record SearchResultDocument
{
    [JsonIgnore]
    public virtual string Id { get; private set; }

    [JsonIgnore]
    public virtual Dictionary<string, object> Fields { get; } = new();

    [JsonProperty("_meta")]
    public virtual SearchResultDocumentMetadata Meta { get; init; }

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
