using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Curations;

public class Curation
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("queries")]
    public string[] Queries { get; set; }

    [JsonProperty("promoted")]
    public JArray Promoted { get; set; }

    [JsonProperty("hidden")]
    public JArray Hidden { get; set; }

    [JsonProperty("organic")]
    public JArray Organic { get; set; }
}
