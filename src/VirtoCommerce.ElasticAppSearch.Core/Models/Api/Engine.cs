using System.Text.Json.Serialization;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api;

public record Engine
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public EngineType Type { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; }
}
