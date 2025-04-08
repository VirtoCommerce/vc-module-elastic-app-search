using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Curations;

public class Curation
{
    public string Id { get; set; }

    public string[] Queries { get; set; }

    public JArray Promoted { get; set; }

    public JArray Hidden { get; set; }

    public JArray Organic { get; set; }
}
