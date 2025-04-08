using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Pagination;

public class PaginatedResponse
{
    public Meta Meta { get; set; }

    public JArray Results { get; set; }
}
